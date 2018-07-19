using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntegraLib
{
    /// <summary>
    /// Класс, который создает двухуровневый словарь с символьными ключами.
    /// Значения могут быть какие угодно.
    /// Помогает получить как Dictionary, так и набор XElement-ов
    /// </summary>
    public class Dict2LayerHelper
    {
        private Dictionary<string, Dictionary<string, object>> _dict;

        public Dict2LayerHelper()
        {
            _dict = new Dictionary<string, Dictionary<string, object>>();
        }

        public void AddLayer1Item(string key)
        {
            lock (_dict)
            {
                if (_dict.ContainsKey(key) == false) _dict.Add(key, new Dictionary<string, object>());
            }
        }

        public void AddLayer2Item(string key1, string key2, object value)
        {
            lock (_dict)
            {
                if (_dict.ContainsKey(key1) == false) _dict.Add(key1, new Dictionary<string, object>());

                Dictionary<string, object> dict2 = _dict[key1];
                if (dict2.ContainsKey(key2))
                    dict2[key2] = value;
                else
                    dict2.Add(key2, value);
            }
        }

        public void AddLayer2ParamsKVP(string key1, params KeyValuePair<string, object>[] kvpArray)
        {
            lock (_dict)
            {
                if (_dict.ContainsKey(key1) == false) _dict.Add(key1, new Dictionary<string, object>());

                foreach (KeyValuePair<string, object> item in kvpArray)
                {
                    AddLayer2Item(key1, item.Key, item.Value);
                }
            }
        }

        public Dictionary<string, Dictionary<string, object>> ToDictionary()
        {
            return _dict;
        }

        public List<XElement> ToXElementList()
        {
            List<XElement> retVal = new List<XElement>();

            lock (_dict)
            {
                foreach (KeyValuePair<string, Dictionary<string, object>> item1 in _dict)
                {
                    XElement xe = new XElement(item1.Key);
                    foreach (KeyValuePair<string, object> itemAttr in item1.Value)
                    {
                        xe.Add(new XAttribute(itemAttr.Key, itemAttr.Value));
                    }
                    retVal.Add(xe);
                }

            }

            return (retVal.Count == 0) ? null : retVal;
        }

    }  // class
}
