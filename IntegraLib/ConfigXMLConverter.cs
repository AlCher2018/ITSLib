using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntegraLib
{
    /// <summary>
    /// Преобразование формата файлов *.config в XML.
    /// *.config-файл состоит из одних элементов add с атрибутами key (параметр приложения) и value (значение параметра)
    ///   <!-- Таймаут запуска, в секундах, по умолчанию - 0 секунд -->
    ///   <add key = "StartTimeout" value="0"/>
    /// XML-файл состоит из элементов, имена которых есть ГРУППА параметров приложения.
    /// Атрибут элемента XML-файла - это параметр приложения и его значение.
    /// 
    /// Данный класс служит для преобразования config-файла в XML и наоборот.
    /// </summary>
    public static class ConfigXMLConverter
    {
        #region .config to XML

        // является ли XML-документ валидным config-файлом?
        public static bool IsValidConfigFile(XDocument xDoc)
        {
            bool retVal = true;

            foreach (XElement item in xDoc.Root.Elements())
            {
                if (item.Name.LocalName != "add") { retVal = false; break; }

                IEnumerable<XAttribute> attrs = item.Attributes();
                if (attrs.Count() != 2) { retVal = false; break; }
                if ((attrs.ElementAt(0).Name.LocalName != "key") && (attrs.ElementAt(1).Name.LocalName != "value")) { retVal = false; break; }
            }

            return retVal;
        }

        // преобразование элементов  
        //      <add key="AppParamName" value="AppParamValue"/>
        // в
        //      <AppParamName value="AppParamValue"/>
        // все комментарии перед <add ... /> сохраняются, как вложенные элементы comment
        public static XDocument ConvertConfigToXML(XDocument xConfigDoc)
        {
            List<XElement> keys = new List<XElement>();
            List<XComment> comments = null;
            foreach (XNode node in xConfigDoc.Root.Nodes())
            {
                if (node is XComment)
                {
                    if (comments == null) comments = new List<XComment>();
                    comments.Add(node as XComment);
                }
                else if (node is XElement)
                {
                    XElement xElFrom = (node as XElement);
                    keys.Add(new XElement(xElFrom.Attribute("key").Value, xElFrom.Attribute("value"), comments));
                    if (comments != null) comments = null;
                }
            }

            return new XDocument(xConfigDoc.Declaration, new XElement(xConfigDoc.Root.Name, xConfigDoc.Root.Attributes(), keys));
        }

        #endregion

        #region XML to .config

        // все элементы содержат: все вложенные элементы - XComments, атрибут только один и его имя - value
        public static bool CanConvertToConfig(XDocument xDoc)
        {
            return xDoc.Root.Elements().All(e =>
            {
                // все вложенные ноды - XComment
                bool b1 = (e.Nodes().Count() == 0);
                if (!b1) b1 = e.Nodes().All(i => (i is XComment));

                XAttribute[] attrs = e.Attributes().ToArray();
                return (b1)
                    && (attrs.Length == 1)
                    && (attrs[0].Name.LocalName == "value");
            });
        }

        public static XDocument ConvertXMLToConfig(XDocument xDoc)
        {
            List<XNode> nodes = new List<XNode>();

            foreach (XElement e in xDoc.Root.Elements())
            {
                // вложенные ноды - это комментарии, которые должны идти перед элементом
                XAttribute[] attrs = e.Attributes().ToArray();
                if ((attrs.Length == 1) && (attrs[0].Name.LocalName == "value"))
                {
                    // комментарии
                    if (e.Nodes().Count() != 0) nodes.AddRange(e.Nodes());
                    // элемент
                    nodes.Add(new XElement("add", new XAttribute("key", e.Name.LocalName), attrs[0]));
                }
            }

            return new XDocument(xDoc.Declaration, new XElement(xDoc.Root.Name, xDoc.Root.Attributes(), nodes));
        }

        #endregion

    } // class
}
