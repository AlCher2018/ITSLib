using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IntegraLib
{
    public class RegistryHelper
    {
        private const string companyName = "Integra";
        private const string autoGenParamName = "autoGenLicence";

        private static object locker = new object();

        private static string _errMsg = null;
        public static string ErrMsg { get { return _errMsg; } }


        // subFolder можно задавать в виде subFolder1/subFolder2/subFolder3..., где в качестве разделителя может быть \, /, |
        public static string GetCompanyValue(string subFolder, string keyName)
        {
            lock (locker)
            {
                string retVal = null;
                if (_errMsg != null) _errMsg = null;
                RegistryKey integraFolder = getCompanyFolder(false);
                // если нет фолдера компании, то создать его
                if (integraFolder == null) integraFolder = createCompanyFolder();

                if (integraFolder != null)
                {
                    RegistryKey targetFolder = integraFolder;
                    try
                    {
                        // если есть субфолдер, то взять значение из него
                        if (!subFolder.IsNull())
                        {
                            string[] subFolders = getSubfolders(subFolder);
                            foreach (string item in subFolders)
                            {
                                if (targetFolder.GetSubKeyNames().Contains(item))
                                    targetFolder = targetFolder.OpenSubKey(item, false);
                                else
                                    break;
                            }
                        }

                        if (targetFolder != null)
                        {
                            if (targetFolder.GetValueNames().Contains(keyName))
                            {
                                var value = targetFolder.GetValue(keyName);
                                if (value is byte[])
                                {
                                    retVal = (value as byte[]).ToASCIIString();
                                }
                                else if (value is string[])
                                {
                                    retVal = string.Join(Environment.NewLine, (value as string[]));
                                }
                                else
                                {
                                    retVal = value.ToString();
                                }
                            }
                            targetFolder.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errMsg = ex.ToString();
                    }
                }

                return retVal;
            }
        }

        // subFolder можно задавать в виде subFolder1/subFolder2/subFolder3..., где в качестве разделителя может быть \, /, |
        public static bool SetCompanyValue(string subFolder, string keyName, string keyValue)
        {
            lock (locker)
            {
                if (_errMsg != null) _errMsg = null;

                RegistryKey integraFolder = getCompanyFolder(true);
                // если нет фолдера компании, то создать его
                if (integraFolder == null) integraFolder = createCompanyFolder();

                if (integraFolder != null)
                {
                    // если есть субфолдер, то проверить его и, если надо, создать
                    try
                    {
                        RegistryKey targetFolder = integraFolder;
                        if (subFolder.IsNull() == false)
                        {
                            string[] subFolders = getSubfolders(subFolder);
                            foreach (string item in subFolders)
                            {
                                if (targetFolder.GetSubKeyNames().Contains(item))
                                    targetFolder = targetFolder.OpenSubKey(item, true);
                                else
                                {
                                    targetFolder = targetFolder.CreateSubKey(item, RegistryKeyPermissionCheck.ReadWriteSubTree);
                                }
                            }
                        }

                        if (targetFolder != null)
                        {
                            if (keyName.IsNull())
                            {
                                _errMsg = "Имя ключа не может быть пустым!";
                            }
                            else
                            {
                                targetFolder.SetValue(keyName, (keyValue??""));
                            }
                            targetFolder.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errMsg = ex.ToString();
                    }
                }

                return (_errMsg == null);
            }
        }

        private static string[] getSubfolders(string folderName)
        {
            return folderName.Split(new char[] {'\\', '/', '|'});
        }

        public static bool IsExistsCompanyKey()
        {
            bool retVal = false;
            using (RegistryKey currentUserKey = Registry.CurrentUser)
            {
                using (RegistryKey software = currentUserKey.OpenSubKey("Software", true))
                {
                    // проверка наличия ключа Integra
                    retVal = (software.GetSubKeyNames().Contains(companyName));
                    software.Close();
                }
                currentUserKey.Close();
            }
            return retVal;
        }

        private static RegistryKey getCompanyFolder(bool writable = false)
        {
            RegistryKey retVal = null;

            try
            {
                if (_errMsg != null) _errMsg = null;

                using (RegistryKey currentUserKey = Registry.CurrentUser)
                {
                    using (RegistryKey software = currentUserKey.OpenSubKey("Software", true))
                    {
                        // проверка наличия ключа Integra
                        if (software.GetSubKeyNames().Contains(companyName))
                        {
                            retVal = software.OpenSubKey(companyName, writable);
                        }
                        software.Close();
                    }
                    currentUserKey.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.ToString();
            }

            return retVal;
        }

        private static RegistryKey createCompanyFolder()
        {
            RegistryKey retVal = null;
            try
            {
                if (_errMsg != null) _errMsg = null;

                using (RegistryKey currentUserKey = Registry.CurrentUser)
                {
                    using (RegistryKey software = currentUserKey.OpenSubKey("Software", true))
                    {
                        // проверка наличия ключа Integra
                        if (!software.GetSubKeyNames().Contains(companyName))
                        {
                            retVal = software.CreateSubKey(companyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                        }
                        software.Close();
                    }
                    currentUserKey.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.ToString();
            }
            return retVal;
        }

        // проверка наличия в ключе реестра HKLM\Software\Integra\ параметра autoGetLicence
        public static bool IsExistsAutoGenLicenceKey()
        {
            bool retVal = false;
            string sBuf = GetCompanyValue(null, autoGenParamName);
            if (sBuf != null)
            {
                // в массив байтов
                byte[] bytes = Encoding.ASCII.GetBytes(sBuf);

                if (bytes.Length > 0) retVal = (bytes[0] == 1);
            }
            return retVal;
        }

    }  // class
}
