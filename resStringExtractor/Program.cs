using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace resStringExtractor
{
    // файлы .resx читает ResXResourceReader (сборка System.Windows.Form)
    // ResXResourceReader resXReader = new ResXResourceReader("d:\\Resources.resx");

    // файлы .resources читает ResourceReader (сборка mscorlib)
    // C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\mscorlib.dll
    // ResourceReader resReader = new ResourceReader("D:\\strRes.resources");

    // Файлы .resx, скомпилированные как Embedded Resources, переводятся в формат .resources и находятся в манифесте.
    // Читаются в IDictionary<string, string> -> {key, value}
    // Любой файл, скомпилированный с Build Action = Recource, переводится в байтовый поток UnmanagedMemoryStream
    // и читается в строку через StreamReader().ReadToEnd()

    class Program
    {
        // {resources_file, {{key, value}, ...}}
        private static Dictionary<string, Dictionary<string, string>> _resDict;

        static void Main(string[] args)
        {
            // help
            if (args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("String Resources Extractor - программа для просмотра строковых ресурсов из .resx, .resources, .exe, .dll файлов.");
                Console.WriteLine();
                Console.WriteLine("Использование:");
                Console.WriteLine();
                Console.WriteLine("   resStringExtractor.exe <входной_файл>");
                Console.WriteLine();
                Console.WriteLine("   где <входной_файл> - обязательный аргумент, полное имя файла, содержащего ресурсы (resx|resources|exe|dll)");
            }

            else
            {
                string fileName = args[0];
                if (System.IO.File.Exists(fileName) == false)
                {
                    Console.WriteLine($"Файл '{fileName}' НЕ существует!");
                }
                else
                {
                    _resDict = new Dictionary<string, Dictionary<string, string>>();
                    FileInfo fi = new FileInfo(fileName);
                    Console.WriteLine("\nПросмотр ресурсов из файла: " + fileName);

                    // обработка файла
                    string fExt = fi.Extension.Substring(1).ToLower();
                    try
                    {
                        if (fExt == "resx")
                            doResXFile(fileName);
                        else if (fExt == "resources")
                            doResourcesFile(fileName);
                        else if ((fExt == "exe") || (fExt == "dll"))
                            doEmbeddedResources(fileName);
                        else
                            Console.WriteLine("Входной файл должен иметь расширение resx, resources, exe или dll.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    if (_resDict.Count == 0)
                    {
                        Console.WriteLine("Файл не содержит строковых ресурсов.");
                    }
                    else
                    {
                        foreach (KeyValuePair<string, Dictionary<string, string>> item in _resDict)
                        {
                            Console.WriteLine($"\n*** файл-источник '{item.Key}'");
                            foreach (KeyValuePair<string, string> keyVal in item.Value)
                            {
                                Console.WriteLine($"\tkey: '{keyVal.Key}', value: '{keyVal.Value}'");
                            }
                        }
                    }
                }
            }

#if DEBUG
            Console.Write(string.Format("{0}{0}Press any key...", Environment.NewLine));
            Console.ReadKey();
#endif
        }

        private static void doEmbeddedResources(string asmFile)
        {
            Assembly asm = Assembly.LoadFrom(asmFile);
            if (asm == null)
                throw new Exception($"Не могу загрузить сборку '{asmFile}'");

            doEmbRes(asm);
        }

        private static void doEmbRes(Assembly asm)
        {
            string[] resNames = asm.GetManifestResourceNames();
            if (resNames.Length == 0)
            {
                Console.WriteLine("\tвстроенные ресурсы НЕ НАЙДЕНЫ");
                return;
            }

            foreach (string resName in resNames)
            {
                _resDict.Add(resName, new Dictionary<string, string>());

                // read resource from resName (embedded .resources file)
                Stream stream = asm.GetManifestResourceStream(resName);
                if (stream == null)
                {
                    _resDict[resName].Add("get_resources", "получен null stream");
                }
                else
                {
                    ResourceReader resReader = null;
                    try
                    {
                        resReader = new ResourceReader(stream);
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    if (resReader == null) continue;

                    foreach (DictionaryEntry item in resReader)
                    {
                        string key = item.Key.ToString();
                        string value = null;

                        if (item.Value is Stream)
                        {
                            if (key.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                            {
                                ResXResourceReader reader = new ResXResourceReader((Stream)item.Value);
                                foreach (DictionaryEntry resItem in reader)
                                {
                                    _resDict[resName].Add(resItem.Key.ToString(), resItem.Value.ToString());
                                }
                                reader.Close();
                            }
                            else
                            {
                                StreamReader reader = new StreamReader((Stream)item.Value);
                                value = reader.ReadToEnd();
                                reader.Dispose();
                                _resDict[resName].Add(key, value);
                            }
                        }
                        else
                        {
                            value = item.Value.ToString();
                            _resDict[resName].Add(key, value);
                        }
                    }
                    resReader.Dispose();

                }
            }
        }

        private static string removeEndString(string source, string removeString)
        {
            string retVal = source;

            if (retVal.ToLower().EndsWith(removeString, StringComparison.OrdinalIgnoreCase))
                retVal = retVal.Substring(0, retVal.Length - removeString.Length);

            return retVal;
        }


        private static void doResourcesFile(string fileName)
        {
            _resDict.Add(fileName, new Dictionary<string, string>());

            ResourceReader reader = new ResourceReader(fileName);
            foreach (DictionaryEntry item in reader)
            {
                _resDict[fileName].Add(item.Key.ToString(), item.Value.ToString());
            }
        }

        private static void doResXFile(string fileName)
        {
            _resDict.Add(fileName, new Dictionary<string, string>());

            ResXResourceReader reader = new ResXResourceReader(fileName);
            foreach (DictionaryEntry item in reader)
            {
                _resDict[fileName].Add(item.Key.ToString(), item.Value.ToString());
            }
            reader.Close();
        }

        internal static string GetFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();

            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }  // class
}
