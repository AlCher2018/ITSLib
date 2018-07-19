using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace IntegraLib
{

    public static class StringExtensions
    {
        // convert string to bool
        public static bool ToBool(this string source)
        {
            bool retValue = false;
            if (string.IsNullOrEmpty(source)) return retValue;

            string sLower = source.ToLower();

            if (sLower.Equals("true") || sLower.Equals("да") || sLower.Equals("yes") || sLower.Equals("истина"))
                retValue = true;
            else
            {
                int iBuf = 0;
                if (int.TryParse(source, out iBuf) == true) retValue = (iBuf != 0);
            }

            return retValue;
        }  // method

        public static double ToDouble(this string source)
        {
            double retValue = 0;
            if (string.IsNullOrEmpty(source)) return retValue;

            double.TryParse(source, out retValue);
            if (retValue == 0)
            {
                if (source.Contains(",")) source = source.Replace(',', '.');

                double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out retValue);
            }
            return retValue;
        }


        public static decimal ToDecimal(this string source)
        {
            decimal retValue = 0m;
            if (string.IsNullOrEmpty(source)) return retValue;

            decimal.TryParse(source, out retValue);
            if (retValue == 0)
            {
                if (source.Contains(",")) source = source.Replace(',', '.');

                decimal.TryParse(source, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out retValue);
            }
            return retValue;
        }


        public static int ToInt(this string source)
        {
            if (source == null) return 0;

            List<char> chList = new List<char>();
            foreach (char c in source)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.DecimalDigitNumber)
                {
                    chList.Add(c);
                }
                else if ((c == '-') && (!chList.Contains('-')))
                {
                    chList.Add(c);
                }
            }

            if (chList.Count == 0)
                return 0;
            else
            {
                string sNum = string.Join("", chList.ToArray());
                int retVal;
                if (int.TryParse(sNum, out retVal) == false) retVal = 0;
                return  retVal;
            }
        }

        public static bool IsNull(this string source)
        {
            return (string.IsNullOrEmpty(source));
        }

        public static bool IsNumber(this string source)
        {
            return source.All(c => char.IsDigit(c));
        }

        public static string LeftString(this string source, int length)
        {
            if (string.IsNullOrEmpty(source) || (length <= 0)) return "";

            if (length < source.Length)
                return source.Substring(0, length);
            else
                return source;
        }

        public static string RightString(this string source, int length)
        {
            if (string.IsNullOrEmpty(source) || (length <= 0)) return "";

            if (length < source.Length)
                return source.Substring(source.Length-length, length);
            else
                return source;
        }


        public static string ToSQLExpr(this string source)
        {
            return (source.IsNull() ? "Null" : source);
        }

        public static bool IsValidEMail(this string source)
        {
            if (source.IsNull()) return false;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            System.Text.RegularExpressions.Match match = regex.Match(source);

            return match.Success;
        }

        // форматирование строки, placeholder for char is $
        public static string ToFormattedString(this string source, string stringFormat)
        {
            if (string.IsNullOrEmpty(source)) return null;
            if (string.IsNullOrEmpty(stringFormat)) return source;

            char[] aSrc = source.ToArray();
            int iSrcChar = 0, srcLen = aSrc.Length - 1;
            char[] aFmt = stringFormat.ToArray();
            StringBuilder sb = new StringBuilder(aFmt.Length);
            foreach (char cFmt in aFmt)
            {
                if (cFmt.Equals('$'))
                {
                    if (iSrcChar <= srcLen)
                    {
                        sb.Append(aSrc[iSrcChar]);
                        iSrcChar++;
                    }
                    else
                        break;
                }
                else
                {
                    sb.Append(cFmt);
                }
            }
            return sb.ToString();
        }

        // ревер строки
        // https://stackoverflow.com/questions/228038/best-way-to-reverse-a-string
        // быстродействие лучше для коротких строк, длина < 20 знаков
        public static string ReverseShortString(this string input)
        {
            char[] charArray = input.ToCharArray();
            int len = input.Length - 1;

            for (int i = 0; i < len; i++, len--)
            {
                charArray[i] ^= charArray[len];
                charArray[len] ^= charArray[i];
                charArray[i] ^= charArray[len];
            }

            return new string(charArray);
        }
        // быстродействие лучше для длинных строк, длина < 20 знаков
        public static string Reverse(this string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }

        // преобразование строки, в которой каждый символ представлен двумя 16-ричными цифрами, в массив байтов
        // "FFD1FFD2" --> {0xFF, 0xD1, 0xFF, 0xD2}
        public static byte[] HexToByteArray(this string hexString)
        {
            System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary shb = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Parse(hexString);
            return shb.Value;
        }

        // преобразование ASCII-строки в массив байтов
        // "12345"  -->  {49, 50, 51, 52, 53}
        public static byte[] ASCIIToByteArray(this string asciiString)
        {
            return Encoding.ASCII.GetBytes(asciiString);
        }

        public static byte[] UnicodeToByteArray(this string unicodeString)
        {
            return Encoding.Unicode.GetBytes(unicodeString);
        }

        public static bool IsExistsUnicodeChar(this string source)
        {
            foreach (char c in source)
            {
                if (c > 0x00FF) return true;
            }
            return false;
        }

    } // class

    public static class IntExtensions
    {
        public static int SetBit(this int bitMask, int bit)
        {
            return (bitMask |= (1 << bit));
        }
        public static int ClearBit(this int bitMask, int bit)
        {
            return (bitMask &= ~(1 << bit));
        }
        public static bool IsSetBit(this int bitMask, int bit)
        {
            int val = (1 << bit);
            return (bitMask & val) == val;
        }

        // convert byte to byte array
        public static byte[] ToByteArray(this byte source)
        {
            return new byte[] { source };
        }
        // convert ushort to byte array
        public static byte[] ToByteArray(this UInt16 source)
        {
            byte[] bytes = BitConverter.GetBytes(source);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return bytes;
        }
        // convert int to byte array
        public static byte[] ToByteArray(this Int32 intValue)
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

            return intBytes;
        }
        // convert long to byte array
        public static byte[] ToByteArray(this Int64 source)
        {
            byte[] bytes = BitConverter.GetBytes(source);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return bytes;
        }

    }  // class

    public static class DoubleExtensions
    {
        public static double Truncate(this double source, int decPointPosition)
        {
            double multer = Math.Pow(10d, decPointPosition);
            return Math.Truncate(source * multer) / multer;
        }
    }

    public static class DecimalExtensions
    {
        public static string ToStringMoneyFormat(this decimal source)
        {
            return source.ToString("#0.00", FormatProviderHelper.NumDecimalFormatProvider);
        }
    }


    public static class DateTimeExtension
    {
        // MS SQL data type Datetime: January 1, 1753, through December 31, 9999
        private static DateTime sqlMinDate = new DateTime(1753, 1, 1);

        public static void SetZero(this DateTime source)
        {
            source = DateTime.MinValue;
        }
        public static bool IsZero(this DateTime source)
        {
            return source.Equals(DateTime.MinValue);
        }
        /// <summary>
        /// Return string "CONVERT(datetime, '{0}', 20)", source.ToString("yyyy-MM-dd HH:mm:ss.fff")"
        /// </summary>
        /// <param name="source">DateTime for converting</param>
        /// <returns></returns>
        public static string ToSQLExpr(this DateTime source)
        {
            if (source < sqlMinDate)
                return "Null";
            else
                return string.Format("CONVERT(datetime, '{0}', 20)", source.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        public static string ToSQLInvariantDate(this DateTime source)
        {
            if (source < sqlMinDate)
                return "Null";
            else
                return "'" + source.ToString("yyyyMMdd") + "'";
        }

    } // class

    public static class TimeSpanExtension
    {
        public static void SetZero(this TimeSpan source)
        {
            source = TimeSpan.Zero;
        }
        public static bool IsZero(this TimeSpan source)
        {
            return source.Equals(TimeSpan.Zero);
        }

        public static DateTime ToDateTime(this TimeSpan source)
        {
            return new DateTime(source.Ticks, DateTimeKind.Local);
        }

        public static int ToIntSec(this TimeSpan source)
        {
            return (int)source.TotalSeconds;
        }

    }  // class

    public static class ArrayExtension
    {
        // преобразование массива байт в строку, где каждый символ представлен двумя 16-ричными цифрами
        // {0xFF, 0xD1, 0xFF, 0xD2}  -->  "FFD1FFD2"
        public static string ToHexString(this byte[] value)
        {
            if ((value == null) || (value.Length == 0)) return null;

            System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary shb = new System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(value);
            return shb.ToString();
        }

        // преобразование массива байтов в ASCII-строку
        // {49, 50, 51, 52, 53}  -->  "12345"
        public static string ToASCIIString(this byte[] value)
        {
            if ((value == null) || (value.Length == 0)) return null;

            return Encoding.ASCII.GetString(value);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        // return byte value (1 byte)
        public static byte ToByte(this byte[] source, int startIndex = 0)
        {
            if (source == null) return 0;
            return source[startIndex];
        }
        // return ushort value (2 bytes)
        public static UInt16 ToUInt16(this byte[] source, int startIndex = 0)
        {
            if ((source == null) || (source.Length < 2)) return 0;
            if (BitConverter.IsLittleEndian)
            {
                byte[] b2 = source.SubArray(startIndex, 2);
                Array.Reverse(b2);
                return BitConverter.ToUInt16(b2, 0);
            }
            else
                return BitConverter.ToUInt16(source, startIndex);
        }
        // return int value (4 bytes)
        public static Int32 ToInt32(this byte[] source, int startIndex = 0)
        {
            if ((source == null) || (source.Length < 4)) return 0;
            if (BitConverter.IsLittleEndian)
            {
                byte[] b4 = source.SubArray(startIndex, 4);
                Array.Reverse(b4);
                return BitConverter.ToInt32(b4, 0);
            }
            else
                return BitConverter.ToInt32(source, startIndex);
        }
        // return long value (8 bytes)
        public static Int64 ToInt64(this byte[] source, int startIndex = 0)
        {
            if (source.Length < 8) return 0;
            if (BitConverter.IsLittleEndian)
            {
                byte[] b8 = source.SubArray(startIndex, 8);
                Array.Reverse(b8);
                return BitConverter.ToInt64(b8, 0);
            }
            else
                return BitConverter.ToInt64(source, startIndex);
        }

    }  // class

    public static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void UIThread(this System.Windows.Forms.Control @this, Action code)
        {
            if (@this.InvokeRequired)
            {
                @this.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }

    public static class DataGridViewExtensions
    {
       // выделить строку в гриде по Id
       public static void SelectRowById(this System.Windows.Forms.DataGridView source, int id)
        {
            if (!source.Columns.Contains("Id")) return;

            foreach (System.Windows.Forms.DataGridViewRow item in source.SelectedRows) item.Selected = false;

            foreach (System.Windows.Forms.DataGridViewRow row in source.Rows)
            {
                if ((int)row.Cells["Id"].Value == id)
                {
                    row.Selected = true;
                    source.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }
    }

}
