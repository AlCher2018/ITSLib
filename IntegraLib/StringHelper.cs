using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public class StringHelper
    {
        #region class static members

        private static StringBuilder _sBuf;

        static StringHelper()
        {
            _sBuf = new StringBuilder();
        }

        public static void SBufClear() { _sBuf.Clear(); }
        public static void SBufAppendText(string text) { _sBuf.Append(text); }
        public static void SBufAppendLine(string text) { _sBuf.AppendLine(text); }

        /// <summary>
        /// Return string from the buffer witout flusing it.
        /// </summary>
        /// <returns></returns>
        public static string SBufReadString() { return _sBuf.ToString(); }

        /// <summary>
        /// Return string from the buffer AND clear the buffer.
        /// </summary>
        /// <returns></returns>
        public static string SBufGetString() { string retVal = SBufReadString(); SBufClear(); return retVal; }

        #endregion

        private StringBuilder _buf;

        private string _tokenDelimiter;
        public string TokenDelimiter
        {
            get { return _tokenDelimiter; }
            set { _tokenDelimiter = value; }
        }


        public StringHelper()
        {
            _buf = new StringBuilder();
        }

        public void AddText(string text)
        {
            if (text == null) return;

            if ((_buf.Length > 0) && (_tokenDelimiter != null)) _buf.Append(_tokenDelimiter);

            _buf.Append(text);
        }

        public override string ToString()
        {
            return _buf.ToString();
        }

    }  // class StringHelper
}
