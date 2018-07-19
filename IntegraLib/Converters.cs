using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public static class Converters
    {

        #region DB value converters
        public static string DBValueToString(object value)
        {
            if (value == DBNull.Value) return null;
            else return Convert.ToString(value);
        }

        public static int DBValueToInt(object value)
        {
            if (value == DBNull.Value) return 0;
            else return Convert.ToInt32(value);
        }
        public static short DBValueToShort(object value)
        {
            if (value == DBNull.Value) return 0;
            else return Convert.ToInt16(value);
        }
        #endregion

        public static string toSQLString(object value)
        {
            if ((value == null) || (value.GetType().Equals(typeof(System.DBNull))))
                return "NULL";
            else if (value is string)
                return string.Format("'{0}'", value.ToString());
            else if (value is bool)
                return string.Format("{0}", ((bool)value ? "1" : "0"));
            else if (value is DateTime)
                return ((DateTime)value).ToSQLExpr();
            else if (value is float)
                return ((float)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else if (value is double)
                return ((double)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else if (value is decimal)
                return ((decimal)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else
                return value.ToString();
        }

    }
}
