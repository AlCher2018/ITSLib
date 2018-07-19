using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace itsCfgUpd
{
    class Program
    {
        // cfg-файлы и команды
        private static Dictionary<string, List<CfgActions>> _cfgActions;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                help();
            else
            {
                _cfgActions = new Dictionary<string, List<CfgActions>>();
                try
                {
                    // parsing args
                    parseArgs(args);

                    // process commands
                    processCommands();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nОШИБКА: " + ex.Message);
                }
            }
#if DEBUG
            Console.Write("\nPress any key..."); Console.ReadKey();
#endif
        }

        private static void processCommands()
        {
            // цикл по config-файлам
            foreach (var item in _cfgActions)
            {
                string cfgFile = item.Key;
                bool isRewriteCfgFile = false;

                if (File.Exists(cfgFile) == false) throw new Exception("Конфигурационный файл '" + cfgFile + "' НЕ найден.");
                Console.WriteLine("\nОбработка файла '" + cfgFile + "'...");

                // чтение файла
                List<string> cfgLines = File.ReadAllLines(cfgFile).ToList();
                string cmdMsg=null;

                // цикл по командам
                #region цикл по командам
                foreach (var itemAction in item.Value)
                {
                    if (itemAction.Command == CfgCmdEnum.Add)
                    {
                        addParam(cfgLines, itemAction);
                        if (itemAction.IsResult) isRewriteCfgFile = true;
                        cmdMsg = " - добавление параметра '" + itemAction.Key + "': " + (itemAction.ResultMsg ?? "-");

                    }
                    else if (itemAction.Command == CfgCmdEnum.Delete)
                    {
                        delParam(cfgLines, itemAction);
                        if (itemAction.IsResult) isRewriteCfgFile = true;
                        cmdMsg = " - удаление параметра '" + itemAction.Key + "': " + (itemAction.ResultMsg ?? "-");
                    }
                    else if (itemAction.Command == CfgCmdEnum.Update)
                    {
                        updParam(cfgLines, itemAction);
                        if (itemAction.IsResult) isRewriteCfgFile = true;
                        string cmdOptions = ((itemAction.Options == null) ? "" : string.Join(" ", itemAction.Options));
                        cmdMsg = " - обновление параметра '" + itemAction.Key + "' (" + cmdOptions + "): " + (itemAction.ResultMsg ?? "-");
                    }
                    else if (itemAction.Command == CfgCmdEnum.Info)
                    {
                        infoParam(cfgLines, itemAction);
                        cmdMsg = " - значение параметра '" + itemAction.Key + "' = " + (itemAction.ResultMsg ?? "-");
                    }
                    else cmdMsg = " - команда '" + itemAction.CmdName + "' не распознана";

                    Console.WriteLine(cmdMsg);
                }
                #endregion

                // переписать config-файл
                if (isRewriteCfgFile)
                {
                    File.WriteAllLines(cfgFile, cfgLines, Encoding.UTF8);
                    Console.WriteLine("Файл '" + cfgFile + "' переписан успешно!");
                }
            }
        }

        private static void updParam(List<string> lines, CfgActions action)
        {
            action.IsResult = false;

            FindParamResult fr = findParam(lines, action.Key);
            if ((fr.IsError) || (fr.Key == null) || (fr.LineIndex == 0))
            {
                action.ResultMsg = fr.Comment;
                return;
            }
            if ((action.Options == null) || (action.Options.Length == 0))
            {
                action.ResultMsg = "не указан обязательный режим обновления: -k[ey] | -v[alue] | -c[omment]";
                return;
            }

            string mode = action.Options[0];
            string newValue = ((action.Options.Length > 1) ? action.Options[1] : "");

            // обновить имя параметра
            if ((mode == "-k") || (mode == "-key"))
            {
                if (newValue.Length == 0)
                {
                    action.ResultMsg = "не указано обязательное новое ИМЯ параметра";
                    return;
                }
                int i1 = fr.Line.IndexOf("key"), i2 = -1;
                if (i1 > -1) i1 = fr.Line.IndexOf("\"", i1 + 3);
                if (i1 > -1) i2 = fr.Line.IndexOf("\"", i1 + 1);
                if ((i1 > -1) && (i2 > -1))
                {
                    string newLine = fr.Line.Substring(0, i1+1) + newValue + fr.Line.Substring(i2);
                     lines[fr.LineIndex] = newLine;
                    action.ResultMsg = "ИМЯ параметра изменено на '" + newValue + "'";
                    action.IsResult = true;
                }
                else
                {
                    action.ResultMsg = "ошибка поиска значения атрибута KEY в исходной строке";
                }
            }
            // обновить значение параметра
            else if ((mode == "-v") || (mode == "-value"))
            {
                if (newValue.Length == 0)
                {
                    action.ResultMsg = "не указано обязательное новое ЗНАЧЕНИЕ параметра";
                    return;
                }
                int i1 = fr.Line.IndexOf("value"), i2 = -1;
                if (i1 > -1) i1 = fr.Line.IndexOf("\"", i1 + 3);
                if (i1 > -1) i2 = fr.Line.IndexOf("\"", i1 + 1);
                if ((i1 > -1) && (i2 > -1))
                {
                    string newLine = fr.Line.Substring(0, i1 + 1) + newValue + fr.Line.Substring(i2);
                    lines[fr.LineIndex] = newLine;
                    action.ResultMsg = "ЗНАЧЕНИЕ параметра изменено на '" + newValue + "'";
                    action.IsResult = true;
                }
                else
                {
                    action.ResultMsg = "ошибка поиска значения атрибута VALUE в исходной строке";
                }
            }
            // обновить комментарий
            else if ((mode == "-c") || (mode == "-comment"))
            {
                // есть комментарий в исходном файле
                if (fr.CommentIndex > 0)
                {
                    // удалить комментарий
                    if (string.IsNullOrEmpty(newValue))
                    {
                        deleteComment(lines, fr);
                        action.ResultMsg = "КОММЕНТАРИЙ удален";
                        action.IsResult = true;
                    }
                    // обновить комментарий
                    else
                    {
                        string prefixLine = getLinePrefix(lines);
                        string newLine = (prefixLine ?? "") + "<!-- " + newValue + " -->";
                        lines[fr.CommentIndex] = newLine;
                        action.ResultMsg = "КОММЕНТАРИЙ изменен";
                        action.IsResult = true;
                    }
                }
                // нет комментария и не задан - ошибка
                else if (string.IsNullOrEmpty(newValue))
                {
                    action.ResultMsg = "КОММЕНТАРИЙ к параметру НЕ найден и НЕ задан";
                }
                // нет комментария и задан - добавить
                else
                {
                    string prefixLine = getLinePrefix(lines);
                    string newLine = (prefixLine ?? "") + "<!-- " + newValue + " -->";
                    lines.Insert(fr.LineIndex, newLine);
                    lines.Insert(fr.LineIndex, "");
                    action.ResultMsg = "КОММЕНТАРИЙ добавлен";
                    action.IsResult = true;
                }
            }

            else
            {
                action.ResultMsg = "ошибочный режим обновления: " + mode + " (должен быть -k[ey], -v[alue] или -c[omment])";
            }
        }

        private static void delParam(List<string> lines, CfgActions action)
        {
            FindParamResult fr = findParam(lines, action.Key);
            if ((fr.IsError) || (fr.Key == null) || (fr.LineIndex == 0))
            {
                action.IsResult = false;
                action.ResultMsg = fr.Comment;
                return;
            }

            lines.RemoveAt(fr.LineIndex);

            // удалить комментарий
            if (fr.CommentIndex != 0)
            {
                deleteComment(lines, fr);
            }
            action.IsResult = true;
            action.ResultMsg = "параметр удален успешно";
        }

        private static void deleteComment(List<string> lines, FindParamResult fr)
        {
            // удалить пустые строки между комментарием и параметром
            int i1 = fr.LineIndex - 1;
            while (i1 > fr.CommentIndex) { lines.RemoveAt(i1); i1--; }

            // удалить комментарий
            lines.RemoveAt(fr.CommentIndex);

            // удалить пустые строки перед комментарием
            int idx = fr.CommentIndex - 1;
            while ((idx > 0) && (idx < lines.Count) && (lines[idx].TrimStart().Length == 0))
            {
                lines.RemoveAt(idx); idx--;
            }
        }

        private static void addParam(List<string> lines, CfgActions action)
        {
            if (string.IsNullOrEmpty(action.Key))
            {
                action.IsResult = false;
                action.ResultMsg = "ошибка добавления параметра: не задан атрибут KEY";
                return;
            }
            if ((action.Options == null) || (action.Options.Length == 0))
            {
                action.IsResult = false;
                action.ResultMsg = "ошибка добавления параметра: не задано значение атрибута VALUE";
                return;
            }
            string newValue = action.Options[0];
            string newComment = (action.Options.Length > 1) ? "<!-- " + action.Options[1] + " -->" : null;

            bool isAdd = false;
            FindParamResult fr = findParam(lines, action.Key);
            // ключ найден - обновить значение
            if (fr.Key != null)
            {
                string lineUpd = fr.Line;
                int i1 = lineUpd.IndexOf("\"", lineUpd.IndexOf("value") + 5);
                int i2 = lineUpd.IndexOf("\"", i1 + 1);
                if ((i1 != -1) && (i2 != -1))
                {
                    string preValue = lineUpd.Substring(i1 + 1, i2 - i1 - 1);
                    if (preValue == newValue)
                    {
                        action.IsResult = false;
                        action.ResultMsg = "ошибка добавления параметра: параметр с таким значением уже существует, добавление не требуется";
                    }
                    else
                    {
                        lineUpd = lineUpd.Substring(0, i1 + 1) + newValue + lineUpd.Substring(i2);
                        lines[fr.LineIndex] = lineUpd;
                        action.IsResult = true;
                        action.ResultMsg = "параметр существует, атрибут VALUE обновлен новым значением";
                    }
                }
                else
                {
                    action.IsResult = false;
                    action.ResultMsg = "ошибка получения значения атрибута VALUE";
                    isAdd = true;
                }
            }
            else
                isAdd = true;

            // ключ не найден или ошибка - добавить
            if (isAdd)
            {
                string newLine = string.Format("<add key=\"{0}\" value=\"{1}\" />", action.Key, newValue);
                string prefixLine = getLinePrefix(lines);
                if (prefixLine != null)
                {
                    newComment = prefixLine + newComment;
                    newLine = prefixLine + newLine;
                }

                // вставить перед последней строкой
                if (lines.Last().TrimStart().StartsWith("</"))
                {
                    lines.Insert(lines.Count - 1, "");
                    if (newComment != null) lines.Insert(lines.Count - 1, newComment);
                    lines.Insert(lines.Count - 1, newLine);
                }
                // добавить к концу
                else
                {
                    lines.Add("");
                    if (newComment != null) lines.Add(newComment);
                    lines.Add(newLine);
                }
                action.IsResult = true;
                action.ResultMsg = "в файл добавлена строка " + newLine + ((newComment==null) ? "" : ", с комментарием " + newComment);
            }
        }

        private static void infoParam(List<string> lines, CfgActions action)
        {
            FindParamResult fr = findParam(lines, action.Key);
            if (fr.IsError)
            {
                action.IsResult = false;
                action.ResultMsg = fr.Comment;
            }
            else
            {
                action.IsResult = true;
                action.ResultMsg = string.Format("'{0}', комментарий: {1}", fr.Value, fr.Comment);
            }
        }

        private static string getLinePrefix(List<string> lines)
        {
            if ((lines == null) || (lines.Count == 0)) return null;

            int index = lines.Count - 1; // с последней строки
            if (lines[index].TrimStart().StartsWith("</")) index--;
            while ((index > 0) && (lines[index].TrimStart().Length == 0)) index--;

            return lines[index].Substring(0, lines[index].IndexOf("<add"));
        }


        private static FindParamResult findParam(List<string> lines, string paramName)
        {
            FindParamResult retVal = new FindParamResult();
            string line = lines.FirstOrDefault(l => l.Contains("\"" + paramName + "\""));
            if (line == null)
            {
                retVal.IsError = true;
                retVal.Comment = "ошибка поиска: ключ НЕ найден";
                return retVal;
            }

            retVal.Line = line;
            retVal.LineIndex = lines.IndexOf(line);

            // атрибут key
            int i1 = line.IndexOf('\"', 0);
            if (i1 <= 0)
            {
                retVal.IsError = true;
                retVal.Comment = "ошибка структуры xml-элемента add: нет ОТКРЫВАЮЩЕЙ кавычки для атрибута KEY";
                return retVal;
            }
            int i2 = line.IndexOf('\"', i1+1);
            if (i2 <= 0)
            {
                retVal.IsError = true;
                retVal.Comment = "ошибка структуры xml-элемента add: нет ЗАКРЫВАЮЩЕЙ кавычки для атрибута KEY";
                return retVal;
            }
            retVal.Key = line.Substring(i1 + 1, i2 - i1 - 1);

            // атрибут value
            i1 = line.IndexOf('\"', i2+1);
            if (i1 <= 0)
            {
                retVal.IsError = true;
                retVal.Comment = "ошибка структуры xml-элемента add: нет ОТКРЫВАЮЩЕЙ кавычки для атрибута VALUE";
                return retVal;
            }
            i2 = line.IndexOf('\"', i1 + 1);
            if (i2 <= 0)
            {
                retVal.IsError = true;
                retVal.Comment = "ошибка структуры xml-элемента add: нет ЗАКРЫВАЮЩЕЙ кавычки для атрибута VALUE";
                return retVal;
            }
            retVal.Value = line.Substring(i1 + 1, i2 - i1 - 1);

            // comment
            // признак комментария для строки: перед этой строкой есть xml-комментарий, и после - есть xml-комментарий.
            // если после строки нет комментария, то это 
            if ((retVal.LineIndex == 0) || (retVal.LineIndex == lines.Count))
            {
                retVal.IsError = true;
                retVal.Comment = "ошибка структуры config-файла: нет корневого элемента";
                return retVal;
            }

            int iLinePre = retVal.LineIndex - 1;
            string linePre = lines.ElementAt(iLinePre).TrimStart();
            while (((linePre == null) || (linePre.Length == 0)) && (iLinePre > 0))
            {
                iLinePre--;
                linePre = lines.ElementAt(iLinePre).TrimStart();
            }
            string comment = ((linePre.StartsWith("<!--")) ? linePre : null);
            // проверка комментария на принадлежность одному параметру или для группы параметров
            if (comment != null)
            {
                int iLinePost = retVal.LineIndex + 1;
                string linePost = lines.ElementAt(iLinePost).TrimStart();
                
                // пропустить последующие пустые строки
                while (((linePost == null) || (linePost.Length == 0)) && (iLinePost < lines.Count))
                {
                    iLinePost++;
                    linePost = lines.ElementAt(iLinePost).TrimStart();
                }

                if ((string.IsNullOrEmpty(linePost) == false)
                    && (linePost.StartsWith("<!--") || linePost.StartsWith("</")))
                {
                    if (comment.StartsWith("<!--")) comment = comment.Substring(4).TrimStart();
                    if (comment.EndsWith("-->")) comment = comment.Substring(0, comment.Length-3).TrimEnd();
                    retVal.Comment = comment;
                    retVal.CommentIndex = iLinePre;
                }
            }

            return retVal;
        }

        private static string getCmdOptions(CfgActions action)
        {
            return (
                (action.Options == null) 
                ? "" 
                : string.Join(", ", action.Options.Select(i => "'" + i + "'"))
            );
        }

        private static void parseArgs(string[] args)
        {
            string cfgFile = null;

            // команды из файла
            if ((args[0] == "-l") || (args[0] == "-list"))
            {
                if (args.Length < 2) throw new Exception("Не указан командный файл.");
                string cmdFile = args[1];
                checkFilePath(ref cmdFile);
                if (File.Exists(cmdFile) == false) throw new Exception("Командный файл '" + cmdFile + "' НЕ найден.");
                string[] lines = File.ReadAllLines(cmdFile, Encoding.Default);
                foreach (string item in lines)
                {
                    string line = item.Trim();
                    if (line.Length == 0) continue;

                    if (line.StartsWith("-"))
                    {
                        if (cfgFile == null) throw new Exception("В первой строке командного файла не указано имя config-файла.");
                        string[] cmds = parseCommandString(line);
                        addCmdToActions(cfgFile, cmds);
                    }
                    else
                    {
                        cfgFile = line;
                        checkCfgFileExtension(ref cfgFile);
                        checkFilePath(ref cfgFile);
                    }
                }
            }
            // команды из аргументов
            else
            {
                cfgFile = args[0];
                checkCfgFileExtension(ref cfgFile);
                checkFilePath(ref cfgFile);

                string[] cmds = null;
                if (args.Length > 1) cmds = args.Skip(1).ToArray();
                addCmdToActions(cfgFile, cmds);
            }

            //int cnt = _cfgActions.Count;
        }

        private static string[] parseCommandString(string line)
        {
            string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<String> stringList = new List<string>();
            string temp = String.Empty;
            foreach (var s in tokens)
            {
                if (!String.IsNullOrWhiteSpace(temp))
                {
                    if (s.EndsWith("\"") && !s.EndsWith(@"\"""))
                    {
                        string item = temp + " " + s.Substring(0, s.Length - 1).Replace(@"\""", @"""");
                        stringList.Add(item);
                        temp = string.Empty;
                    }
                    else
                        temp = temp + " " + s.Replace(@"\""", @"""");
                }
                else if (s.StartsWith("\""))
                {
                    temp = s.Substring(1);
                }
                else
                {
                    stringList.Add(s);
                }
            }

            return stringList.ToArray();
        }

        private static void addCmdToActions(string cfgFile, string[] cmdArgs)
        {
            CfgCmdEnum cmd = CfgCmdEnum.None; string key= null; string[] options = null;
            string cmdName = null;
            if (cmdArgs != null)
            {
                if (cmdArgs.Length > 0)
                {
                    cmdName = cmdArgs[0];
                    cmd = parseArgToCmd(cmdArgs[0]);
                }
                if (cmdArgs.Length > 1) key = cmdArgs[1];
                if (cmdArgs.Length > 2) options = cmdArgs.Skip(2).ToArray();
            }

            CfgActions action = new CfgActions()
            {
                CmdName = cmdName, Command = cmd, Key = key, Options = options
            };

            if (_cfgActions.ContainsKey(cfgFile))
                _cfgActions[cfgFile].Add(action);
            else
                _cfgActions.Add(cfgFile, new List<CfgActions>() { action });
        }

        private static CfgCmdEnum parseArgToCmd(string arg)
        {
            CfgCmdEnum retVal = CfgCmdEnum.None;
            switch (arg)
            {
                case "-a":
                case "-add":
                    retVal = CfgCmdEnum.Add;
                    break;

                case "-d":
                case "-del":
                case "-delete":
                    retVal = CfgCmdEnum.Delete;
                    break;

                case "-r":
                case "-replace":
                    retVal = CfgCmdEnum.Update;
                    break;

                case "-i":
                case "-info":
                    retVal = CfgCmdEnum.Info;
                    break;

                default:
                    break;
            }
            return retVal;
        }

        private static void checkFilePath(ref string file)
        {
            if (file.Contains(@"\") == false)
            {
                string path = (new FileInfo(Assembly.GetExecutingAssembly().Location)).DirectoryName;
                if (path.EndsWith(@"\") == false) path += @"\";
                file = path + file;
            }
        }

        private static void checkCfgFileExtension(ref string cfgFile)
        {
            if (cfgFile.ToLower().EndsWith(".config") == false) cfgFile += ".config";
        }

        private class CfgActions
        {
            internal CfgCmdEnum Command;
            internal string CmdName;
            internal string Key;
            internal string[] Options;

            internal bool IsResult;
            internal string ResultMsg;

            internal string GetCmdParams()
            {
                return string.Format("key: '{0}', params: {1}", Key??"", ((Options==null)?"":string.Join(" ",Options)));
            }
        }

        private enum CfgCmdEnum { None=0, Add, Delete, Update, Info }

        private class FindParamResult
        {
            public string Line;
            public int LineIndex;
            public string Key;
            public string Value;
            public string Comment;
            public int CommentIndex;
            public bool IsError;
        }

        private static void help()
        {
            string helpText = @"Редактирование config-файлов .Net-приложений.
Copyright (C) Integra IT Solutions 2018. Kyiv. http://www.integra-its.com.ua/

Использование:

    itsCfgUpd.exe cfg_file command key [options]
        или
    itsCfgUpd.exe -l cmd_file

    где cfg_file - полное имя конфигурационного файла (файл с расширением .config). Если config-файл находится в папке запуска, то можно указать только имя файла (с или без расширения).
        command - команда редактирования параметра config-файла: добавление, удаление или изменение
        key - имя параметра.

Команды:
    -a[dd] - добавление нового параметра. Параметр добавляется в конец файла.
        опции:
            itsCfgUpd cfg_file -a key value [comment]
        где value - значение параметра,
            comment - необязательный комментарий.
    
    -d[elete] - удаление существующего параметра.
    
    -r[eplace] - замена имени параметра, значения параметра или текста комментария.
        опции:
            itsCfgUpd cfg_file -r key [-k[ey] | -v[alue] | -c[omment]] value
        где value - значение, на которое меняется имя параметра (опция -k), значение параметра (опция -v) или текст комментария (опиция -c). Если value содержит пробелы, то его надо взять в двойные кавычки. Если value содержит двойные кавычки, то перед ними надо ставить обратный слеш (\).
    
    -l[ist] cmd_file - чтение команд из файла,
        где cmd_file - полное имя текстового (кодировка Windows, 1251) файла, который в первой строке содержит полное имя config-файла, а в последующих - команды с соответсвующими опциями. После команд может идти полное имя следующего config-файла с последующими командами и т.д. Т.е. структура командного файла следующая:
            полное имя config-файла
            команды
            ...
            полное имя config-файла
            команды
        Если все config-файлы находятся в папке запуска, то можно указать только имена файлов (с или без расширения).

Примеры:
    itsCfgUpd C:\1c-Paltus\KDS_POVAR_KUH\AppSettings.config -a NextPageText ""продолжение на след.странице"" - добавить в указанный файл параметр NextPageText с указанным значением.
";
            Console.WriteLine(helpText);
        }
    }  // class
}
