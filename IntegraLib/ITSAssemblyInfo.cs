using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public class ITSAssemblyInfo
    {
        #region fields&properties
        private string _assemblyName;
        public string AssemblyName { get { return _assemblyName; } }

        private string _fullName;
        public string FullFileName { get { return _fullName; } }

        private string _fileName;
        public string FileName { get { return _fileName; } }

        private string _fileDir;
        public string FileDir { get { return _fileDir; } }

        private string _fileExt;
        public string FileExt { get { return _fileExt; } }

        private DateTime _dateCreated;
        public DateTime DateCreated { get { return _dateCreated; } }

        private DateTime _dateChanged;
        public DateTime DateChanged { get { return _dateChanged; } }

        private DateTime _dateLastOpened;
        public DateTime DateLastOpened { get { return _dateLastOpened; } }


        private string _version;
        public string Version { get { return _version; } set { _version = value; } }

        private bool _appVersionEnable;
        public bool AppVersionEnable { get { return _appVersionEnable; } set { _appVersionEnable = value; } }

        private System.Reflection.Assembly[] _asemblyArray;

        private string _title;
        public string AssemblyTitle { get { return _title; }}

        private string _description;
        public string AssemblyDescription { get { return _description; } }

        private string _guid;
        public string AssemblyGUID { get { return _guid; } set { _guid = value; } }

        #endregion

        // CTOR
        public ITSAssemblyInfo(string assemblyName = null)
        {
            _asemblyArray = AppDomain.CurrentDomain.GetAssemblies();

            _assemblyName = assemblyName;
            if (_assemblyName == null) _assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            LoadInfo(_assemblyName);
        }


        public void LoadInfo(string assemblyName)
        {
            System.Reflection.Assembly asmFind = _asemblyArray.FirstOrDefault(a => a.FullName.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase));
            if (asmFind != null)
            {
                _fullName = asmFind.ManifestModule.FullyQualifiedName;
                _fileName = asmFind.ManifestModule.Name;
                _fileDir = _fullName.Substring(0, _fullName.LastIndexOf(_fileName));
                int iLast = _fullName.LastIndexOf('.');
                if (iLast > 0) _fileExt = _fullName.Substring(iLast);

                try
                {
                    FileInfo fileInfo = new System.IO.FileInfo(_fullName);
                    _dateCreated = fileInfo.CreationTime;
                    _dateChanged = fileInfo.LastWriteTime;
                    _dateLastOpened = fileInfo.LastAccessTime;
                }
                catch (Exception)
                {
                }

                // version
                string[] s1 = asmFind.FullName.Split(',');
                string[] s2 = s1[1].Trim().Split('=');   // Version=*.*.*.*
                _version = (s2.Length == 1) ? s2[0] : s2[1];
                _appVersionEnable = false;

                // title
                object asmAttr = getAsmAttribute(asmFind, typeof(System.Reflection.AssemblyTitleAttribute));
                if (asmAttr != null) _title = ((System.Reflection.AssemblyTitleAttribute)asmAttr).Title;
                // description
                asmAttr = getAsmAttribute(asmFind, typeof(System.Reflection.AssemblyDescriptionAttribute));
                if (asmAttr != null) _description = ((System.Reflection.AssemblyDescriptionAttribute)asmAttr).Description;
                // guid
                asmAttr = getAsmAttribute(asmFind, typeof(System.Runtime.InteropServices.GuidAttribute));
                if (asmAttr != null) _guid = ((System.Runtime.InteropServices.GuidAttribute)asmAttr).Value;
            }
        }

        private Attribute getAsmAttribute(System.Reflection.Assembly assembly, Type attrType)
        {
            Attribute retVal = null;
            object[] attributes = assembly.GetCustomAttributes(attrType, true);
            if ((attributes != null) && (attributes.Length > 0))
            {
                retVal =  (Attribute)attributes[0];
            }
            return retVal;
        }

        // для сравнения с другими экземплярами, как строками
        // return AssemblyName;GUID if VersionEnable == false
        // return AssemblyName;GUID;Version if VersionEnable == true
        public string ToStringPSW()
        {
            string retVal = (this._assemblyName ?? "") + ";" + (this._guid ?? "");

            if (_appVersionEnable) retVal += ";" + (this._version ?? "");

            return retVal;
        }

        // return AssemblyName;GUID;Version;VersionEnable
        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3}",
                (this._assemblyName ?? ""), (this._guid ?? ""),
                (this._version ?? ""), _appVersionEnable.ToString());
        }

    } // class ITSAssemmblyInfo
}
