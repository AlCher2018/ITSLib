using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    // словарь глобальных свойств
    public static class AppProperties
    {
        private static Dictionary<string, object> _props;

        static AppProperties()
        {
            _props = new Dictionary<string, object>();
        }

        public static void SetProperty(string key, object value)
        {
            lock (_props)
            {
                if (_props.ContainsKey(key))
                {
                    _props[key] = value;
                }
                else
                {
                    _props.Add(key, value);
                }
            }
        }

        public static object GetProperty(string key)
        {
            object retVal = null;
            lock (_props)
            {
                if (_props.ContainsKey(key)) retVal = _props[key];
            }
            return retVal;
        }

        public static bool GetBoolProperty(string key)
        {
            bool retVal = false;
            lock (_props)
            {
                if (_props.ContainsKey(key)) retVal = Convert.ToBoolean(_props[key]);
            }
            return retVal;
        }

        public static int GetIntProperty(string key)
        {
            int retVal = 0;
            lock (_props)
            {
                if (_props.ContainsKey(key)) retVal = Convert.ToInt32(_props[key]);
            }
            return retVal;
        }
        public static double GetDoubleProperty(string key)
        {
            double retVal = 0d;
            lock (_props)
            {
                if (_props.ContainsKey(key)) retVal = Convert.ToDouble(_props[key]);
            }
            return retVal;
        }

        public static void DeleteProperty(string key)
        {
            lock (_props)
            {
                if (_props.ContainsKey(key)) _props.Remove(key);
            }
        }

        public static bool ContainsKey(string key)
        {
            return _props.ContainsKey(key);
        }

    }  // class
}
