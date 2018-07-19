using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public static class AppEnvironment
    {
        public static readonly string DTFormatForDB = "yyyy-MM-dd HH:mm:ss";


        public static string GetEnvironmentString()
        {
            return string.Format("Environment: machine={0}, user={1}, current directory={2}, OS version={3}, isOS64bit={4}, processor count={5}, free RAM={6} Mb",
                Environment.MachineName, Environment.UserName, Environment.CurrentDirectory, Environment.OSVersion, Environment.Is64BitOperatingSystem, Environment.ProcessorCount, getAvailableRAM());
        }

        // проверка существования фолдера
        // также, проверяется наличие в конце строки обратного слеша
        // Return true if folder is exists and in result - folder
        // else if return false, then in result lays error message
        public static bool CheckFolder(string folder, out string result)
        {
            result = null;
            if (folder.IsNull()) return false;

            bool retVal = false;
            if (folder.EndsWith("\\") == false) folder += "\\";

            try
            {
                if (System.IO.Directory.Exists(folder) == false)
                {
                    result = $"Folder '{folder}' is not exists.";
                }
                else
                {
                    result = folder;
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                result = "System.IO error: " + ex.Message;
            }

            return retVal;
        }

        #region app info
        public static string GetAppFileName()
        {
            return System.AppDomain.CurrentDomain.FriendlyName;
        }

        public static string GetAppFullFile()
        {
            string path = null;
            // C:/.../...
            //string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            //UriBuilder uri = new UriBuilder(codeBase);
            //path = Uri.UnescapeDataString(uri.Path);

            // C:\\...\\...
            //var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //path = assembly.Location;

            path = GetAppDirectory() + GetAppFileName();

            return path;
        }


        // AppDomain.CurrentDomain.BaseDirectory - со слешем в конце
        // Directory.GetCurrentDirectory() - без слеша в конце
        public static string GetAppDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetAppDirectory(string subDir)
        {
            return GetAppDirectory() + subDir + ((subDir.EndsWith("\\")) ? "" : "\\");
        }

        public static string GetFullFileName(string relPath, string fileName)
        {
            return getFullPath(relPath) + fileName;
        }
        private static string getFullPath(string relPath)
        {
            string retVal = relPath;

            if (string.IsNullOrEmpty(relPath))  // путь не указан в конфиге - берем путь приложения
                retVal = GetAppDirectory();
            else if (retVal.Contains(@"\:") == false)  // относительный путь
            {
                retVal = GetAppDirectory() + retVal;
            }
            if (retVal.EndsWith(@"\") == false) retVal += @"\";

            return retVal;
        }

        public static string GetAppVersion()
        {
            string path = GetAppFullFile();
            return GetFileVersion(path);
        }

        public static string GetAppAssemblyName()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
        }
        private static System.Reflection.Assembly getAppAssembly()
        {
            string assemblyName = GetAppAssemblyName();
            System.Reflection.Assembly[] asmList = AppDomain.CurrentDomain.GetAssemblies();
            return asmList.FirstOrDefault(a => a.FullName.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetAppGuid()
        {
            System.Reflection.Assembly asm = getAppAssembly();
            if (asm == null) return null;

            string appGUID = null;
            object[] attributes = asm.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
            if ((attributes != null) && (attributes.Length > 0))
            {
                System.Runtime.InteropServices.GuidAttribute ga = (System.Runtime.InteropServices.GuidAttribute)attributes[0];
                appGUID = ga.Value;
            }
            return appGUID;
        }

        // информация о сборке для заголовка приложения, включает в себя Company, Product, Version
        public static string GetAssemblyInfoForAppTitle()
        {
            string company = null, product = null, version = null;
            System.Reflection.Assembly runAssembly = System.Reflection.Assembly.GetEntryAssembly();
            object[] attribs = runAssembly.GetCustomAttributes(true);
            foreach (object item in attribs)
            {
                if (item is System.Reflection.AssemblyCompanyAttribute)
                {
                    company = ((System.Reflection.AssemblyCompanyAttribute)item).Company;
                    continue;
                }

                if (item is System.Reflection.AssemblyProductAttribute)
                {
                    product = ((System.Reflection.AssemblyProductAttribute)item).Product;
                    continue;
                }

                // file version
                if (item is System.Reflection.AssemblyFileVersionAttribute)
                {
                    version = ((System.Reflection.AssemblyFileVersionAttribute)item).Version;
                    continue;
                }

            }

            StringHelper sBuf = new StringHelper() { TokenDelimiter = ", " };
            if (!string.IsNullOrEmpty(company)) sBuf.AddText(company);
            if (!string.IsNullOrEmpty(product)) sBuf.AddText(product);
            if (!string.IsNullOrEmpty(version)) sBuf.AddText("ver. " + version);
            //retVal += getAppVersionAsString(); // assembly version instead
            string retVal = sBuf.ToString();

            return retVal;
        }

        private static string getAppVersionAsString()
        {
            System.Reflection.Assembly runAssembly = System.Reflection.Assembly.GetEntryAssembly();
            System.Reflection.AssemblyName assName = runAssembly.GetName();
            Version ver = assName.Version;

            return string.Format("{0}.{1}.{2}.{3}", ver.Major.ToString(), ver.Minor.ToString(), ver.Build.ToString(), ver.Revision.ToString());
        }
        #endregion

        #region this library info
        public static string GetIntegraLibFullFileName()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.Location;
        }

        public static string GetIntegraLibFileVersion()
        {
            string libFileName = GetIntegraLibFullFileName();
            return GetFileVersion(libFileName);
        }
        #endregion

        // полный путь к специальному файлу с расширением fileExtension в папке приложения
        // если fileName не указан, то берется имя приложения
        public static string GetFullSpecialFileNameInAppDir(string fileExtension, string fileName = null, bool isRemoveExtension = false)
        {
            if (fileName.IsNull()) fileName = GetAppFileName();

            string fDotExt = "." + fileExtension;
            // расширение уже есть
            if (fileName.EndsWith(fDotExt))
            {
                return GetAppDirectory() + fDotExt;
            }
            
            // удалить расширение (после последней точки)
            if (isRemoveExtension)
            {
                int i = fileName.LastIndexOf('.'); if (i > 0) fileName = fileName.Substring(0, i);
                if (fileName.EndsWith(".vshost", StringComparison.OrdinalIgnoreCase)) fileName = fileName.Remove(fileName.Length - 7, 7);
            }

            if (!fileName.EndsWith(fDotExt, StringComparison.OrdinalIgnoreCase)) fileName += fDotExt;

            return GetFullFileName(null, fileName);
        }

        public static string GetFileVersion(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                return fvi.FileVersion;
            }
            else
                return null;
        }

        // in Mb
        public static int getAvailableRAM()
        {
            int retVal = 0;

            // class get memory size in kB
            System.Management.ManagementObjectSearcher mgmtObjects = new System.Management.ManagementObjectSearcher("Select * from Win32_OperatingSystem");
            foreach (var item in mgmtObjects.Get())
            {
                //System.Diagnostics.Debug.Print("FreePhysicalMemory:" + item.Properties["FreeVirtualMemory"].Value);
                //System.Diagnostics.Debug.Print("FreeVirtualMemory:" + item.Properties["FreeVirtualMemory"].Value);
                //System.Diagnostics.Debug.Print("TotalVirtualMemorySize:" + item.Properties["TotalVirtualMemorySize"].Value);
                retVal = (Convert.ToInt32(item.Properties["FreeVirtualMemory"].Value)) / 1024;
            }
            return retVal;
        }

        public static void RestartApplication(string args = null)
        {
            System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess();

            System.Diagnostics.ProcessStartInfo pInfo = new System.Diagnostics.ProcessStartInfo();
            //pInfo.Arguments = string.Format("/C \"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location);
            //pInfo.FileName = "cmd.exe";
            pInfo.FileName = GetAppFullFile();
            if (args.IsNull() == false) pInfo.Arguments = args;

            System.Diagnostics.Process.Start(pInfo);

            curProcess.Kill();
        }

        // открыть ресурс (файл, инет-адрес и т.д.)
        public static void OpenURI(string uri)
        {
            System.Diagnostics.Process.Start(uri);
            //Process p = new Process();
            //p.StartInfo = new ProcessStartInfo(uri);
            //p.Start();
        }


        // проверка и удаление лишних архивных файлов журнала
        // возвращает имена удаленных файлов
        public static List<string> CheckLogFilesCount(int maxLogFiles)
        {
            if (maxLogFiles == 0) return null;

            List<string> retVal = new List<string>();
            string logsPath = AppEnvironment.GetAppDirectory("Logs");
            System.IO.DirectoryInfo logsDir = new System.IO.DirectoryInfo(logsPath);
            System.IO.FileInfo[] logFiles = logsDir.GetFiles("*", System.IO.SearchOption.TopDirectoryOnly);
            if (logFiles.Length > maxLogFiles)
            {
                // отсортировать по возрастанию даты создания файла
                // и удалить первые (logFiles.Length - maxLogFiles) файлов
//                Array.Sort<System.IO.FileInfo>(logFiles, new FileInfoComparer());
//                Array.Sort<System.IO.FileInfo>(logFiles, fileInfoComparision);
                System.IO.FileInfo[] files = (from f in logFiles orderby f.LastWriteTime ascending select f).Take(logFiles.Length - maxLogFiles).ToArray();
                foreach (System.IO.FileInfo item in files)
                {
                    try
                    {
                        item.Delete();
                        retVal.Add(item.Name);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return retVal;
        }

        public static int fileInfoComparision(System.IO.FileInfo x, System.IO.FileInfo y)
        {
            if (x.LastWriteTime < y.LastWriteTime) return -1;
            else if (x.LastWriteTime > y.LastWriteTime) return 1;
            else return 0;
        }

    }  // class AppEnvironment

    public class FileInfoComparer : IComparer<System.IO.FileInfo>
    {
        public int Compare(System.IO.FileInfo x, System.IO.FileInfo y)
        {
            if (x.LastWriteTime < y.LastWriteTime) return -1;
            else if (x.LastWriteTime > y.LastWriteTime) return 1;
            else return 0;
        }
    }
}
