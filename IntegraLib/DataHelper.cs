using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public static class DataHelper
    {
        public static void PopulateObjectByDataRowFunc<T>(T obj, Func<int, DataRow> getDataRowFunc, int id)
        {
            DataRow dr = getDataRowFunc(id);
            if (dr != null)
            {
                Type t = typeof(T);
                PropertyInfo[] pInfo = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo pi in pInfo)
                {
                    if (dr.Table.Columns.Contains(pi.Name) && !dr.IsNull(pi.Name))
                    {
                        pi.SetValue(obj, dr[pi.Name]);
                    }
                }
            }
        }

    }  // class
}
