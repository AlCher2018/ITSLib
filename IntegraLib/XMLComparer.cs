using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace IntegraLib
{

    /// <summary>
    /// Класс для сравнения двух XML-документов
    /// Результат сравнения - набор объектов XMLCompareChangeItem.
    /// Каждый элемент XMLCompareChangeItem содержат описание одного изменения в документе-назначении (_xDocDest)
    /// по сравнению с документом-источником (_xDocSrc)
    /// </summary>
    public class XMLComparer
    {
        // сравниваемые документы
        private XDocument _xDocSrc, _xDocDest;

        private List<XMLCompareChangeItem> _changes;
        public List<XMLCompareChangeItem> Changes { get { return _changes; } }

        private string _errMsg;
        public string ErrorMessage { get { return _errMsg; } }

        public XMLComparer(XDocument xDocSrc, XDocument xDocDest)
        {
            _xDocSrc = xDocSrc;
            _xDocDest = xDocDest;

            _changes = new List<XMLCompareChangeItem>();
        }

        #region compare
        public bool Compare()
        {
            if (_errMsg != null) _errMsg = null;
            _changes.Clear();
            if ((_xDocSrc == null) || (_xDocDest == null)) return false;

            compareXElements(_xDocSrc.Root, _xDocDest.Root);

            return (_errMsg == null);
        }

        private void compareXElements(XElement xESrc, XElement xEDst)
        {
            // проверка атрибутов
            compareAttributes(xESrc, xEDst);

            // и вложенных элементов
            XElement curEl;
            // удаление элементов из Dest
            List<string> delElements = xEDst.Elements().Select(e => e.Name.LocalName).Except(xESrc.Elements().Select(e => e.Name.LocalName)).ToList();
            foreach (string item in delElements)
            {
                XMLCompareChangeItem newItem = new XMLCompareChangeItem();
                newItem.SetNamesFromXElement(xEDst.Element(item));
                newItem.Result = XMLCompareResultEnum.Remove;
                _changes.Add(newItem);
            }

            // цикл по элементам в Source
            // проверяются только уникальные элементы
            foreach (XElement xSrcElement in xESrc.Elements())
            {
                int cntEls = xEDst.Elements(xSrcElement.Name).Count();

                // добавить в Dest новый элемент
                if (cntEls == 0)
                {
                    XMLCompareChangeItem newItem = new XMLCompareChangeItem();
                    newItem.SetNamesFromXElement(xSrcElement);
                    newItem.Result = XMLCompareResultEnum.AddNew;
                    _changes.Add(newItem);
                }

                // проверяются только уникальные элементы
                else if (cntEls == 1)
                {
                    curEl = xEDst.Element(xSrcElement.Name);
                    compareXElements(xSrcElement, curEl);
                }
            }
        }

        // проверка атрибутов
        private void compareAttributes(XElement xESrc, XElement xEDst)
        {
            // - удаление
            List<string> delElements = xEDst.Attributes().Select(e => e.Name.LocalName).Except(xESrc.Attributes().Select(e => e.Name.LocalName)).ToList();
            foreach (string item in delElements)
            {
                XMLCompareChangeItem newItem = new XMLCompareChangeItem();
                newItem.SetNamesFromXElement(xEDst);
                newItem.AttrName = item;
                newItem.Result = XMLCompareResultEnum.Remove;
                _changes.Add(newItem);
            }

            XAttribute curAtr;
            // - цикл по атрибутам в Source
            foreach (XAttribute xAttrSrc in xESrc.Attributes())
            {
                // есть атрибут
                if (xEDst.Attributes(xAttrSrc.Name).Count() > 0)
                {
                    curAtr = xEDst.Attribute(xAttrSrc.Name);
                    if (xAttrSrc.Value != curAtr.Value)
                    {
                        XMLCompareChangeItem newItem = new XMLCompareChangeItem();
                        newItem.SetNamesFromXElement(xESrc);
                        newItem.AttrName = curAtr.Name.LocalName;
                        newItem.Value = xAttrSrc.Value;
                        newItem.Result = XMLCompareResultEnum.ChangeValue;
                        _changes.Add(newItem);
                    }
                }
                // добавить атрибут
                else
                {
                    XMLCompareChangeItem newItem = new XMLCompareChangeItem();
                    newItem.SetNamesFromXElement(xESrc);
                    newItem.AttrName = xAttrSrc.Name.LocalName;
                    newItem.Result = XMLCompareResultEnum.AddNew;
                    _changes.Add(newItem);
                }
            }
        }

        #endregion

        #region update
        public bool Update(List<XMLCompareChangeItem> changes = null)
        {
            if (changes == null) changes = _changes;
            if (changes == null) return false;

            bool result;
            foreach (XMLCompareChangeItem item in changes)
            {
                result = true;

                if (item.Result == XMLCompareResultEnum.AddNew)
                {
                    if (string.IsNullOrEmpty(item.AttrName))
                        result = addNewElement(item);
                    else
                        result = addNewAttribute(item);
                }
                else if (item.Result == XMLCompareResultEnum.Remove)
                {
                    if (string.IsNullOrEmpty(item.AttrName))
                        result = removeElement(item);
                    else
                        result = removeAttribute(item);
                }
                // изменение значения атрибута
                else if (item.Result == XMLCompareResultEnum.ChangeValue)
                {
                    result = changeAttributeValue(item);
                }

                if (!result) return false;
            }

            return true;
        }

        private bool addNewElement(XMLCompareChangeItem changeItem)
        {
            // найти в источнике добавляемый элемент
            XElement xeAfter = findElement(_xDocSrc, changeItem.Names);
            if (xeAfter == null)
            {
                _errMsg = "Not found element '" + getPathFromList(changeItem.Names) + "' in source XML-document which ADD";
                return false;
            }

            // добавить в addItems все элементы перед добавляемым, там могут быть, напр. комментарии
            List<XNode> addItems = new List<XNode>();
            addItems.Add(xeAfter);
            XNode srcNodeAfter = xeAfter.PreviousNode;
            while ((srcNodeAfter != null) && !(srcNodeAfter is XElement))
            {
                addItems.Insert(0, srcNodeAfter);
                srcNodeAfter = srcNodeAfter.PreviousNode;
            }

            // найти в _xDocDest элемент curNode, после которого надо вставить элементы из addItems
            List<string> destParentPath = getNamesFromXElement((XElement)srcNodeAfter);
            XElement xeDestAfter = findElement(_xDocDest, destParentPath);
            if (xeDestAfter == null)
            {
                _errMsg = "Not found element '" + getPathFromList(destParentPath) + "' in destination XML-document for ADD";
                return false;
            }

            // и вставить после xeDestAfter элементы из addItems
            xeDestAfter.AddAfterSelf(addItems);

            return true;
        }

        private bool addNewAttribute(XMLCompareChangeItem changeItem)
        {
            // в источнике: 
            // - найти элемент
            XElement xeSrc = findElement(_xDocSrc, changeItem.Names);
            if (xeSrc == null)
            {
                _errMsg = "Not found element '" + getPathFromList(changeItem.Names) + "' in source XML-document which ADD";
                return false;
            }
            // - найти атрибут, который надо вставить и его индекс
            int xaSrcAfterIndex = findAttributeIndex(xeSrc, changeItem.AttrName);
            if (xaSrcAfterIndex < 0)
            {
                _errMsg = "Not found attribute '" + changeItem.AttrName + "' in the element '" + getPathFromList(changeItem.Names) + "' in the destination XML-document which ADD";
                return false;
            }
            XAttribute xaNew = xeSrc.Attributes().ElementAt(xaSrcAfterIndex);

            // в назначении:
            XElement xeDest = findElement(_xDocDest, changeItem.Names);
            if (xeDest == null)
            {
                _errMsg = "Not found element '" + getPathFromList(changeItem.Names) + "' in destination XML-document for ADD attribute to";
                return false;
            }

            List<XAttribute> attrs = xeDest.Attributes().ToList();
            xeDest.RemoveAttributes();
            attrs.Insert(xaSrcAfterIndex, xaNew);
            xeDest.Add(attrs);

            return true;
        }

        private bool removeElement(XMLCompareChangeItem changeItem)
        {
            // найти в назначении удаляемый элемент
            XElement xElement = findElement(_xDocDest, changeItem.Names);
            if (xElement == null)
            {
                _errMsg = "Not found element '" + getPathFromList(changeItem.Names) + "' in destination XML-document for REMOVE";
                return false;
            }

            xElement.Remove();

            return true;
        }

        private bool removeAttribute(XMLCompareChangeItem changeItem)
        {
            // найти в назначении элемент, в котором удаляется атрибут
            XElement xElement = findElement(_xDocDest, changeItem.Names);
            if (xElement == null)
            {
                _errMsg = "Not found element '" + getPathFromList(changeItem.Names) + "' in destination XML-document for REMOVE attribute from";
                return false;
            }

            // найти удаляемый атрибут
            XAttribute xaDel = findAttribute(xElement, changeItem.AttrName);
            if (xaDel == null)
            {
                _errMsg = "Not found attribute '" + changeItem.AttrName + "' in the element '" + getPathFromList(changeItem.Names) + "' in destination XML-document for REMOVE";
                return false;
            }

            xaDel.Remove();

            return true;
        }

        private bool changeAttributeValue(XMLCompareChangeItem changeItem)
        {
            // найти в назначении элемент, в котором изменяется атрибут
            XElement xElement = findElement(_xDocDest, changeItem.Names);
            if (xElement == null)
            {
                _errMsg = "Not found element '" + getPathFromList(changeItem.Names) + "' in destination XML-document for CHANGE attribute value in";
                return false;
            }

            // найти изменяемый атрибут
            XAttribute xAttribute = findAttribute(xElement, changeItem.AttrName);
            if (xAttribute == null)
            {
                _errMsg = "Not found attribute '" + changeItem.AttrName + "' in the element '" + getPathFromList(changeItem.Names) + "' in destination XML-document for CHANGE its value";
                return false;
            }

            xAttribute.Value = changeItem.Value;

            return true;
        }

        #endregion

        private XElement findElement(XDocument xDoc, List<string> names)
        {
            XElement xeFind = xDoc.Root;
            int i = 0;
            if (xeFind.Name.LocalName == names[0]) i = 1;
            for (; i < names.Count; i++)
            {
                xeFind = xeFind.Element(names[i]);
                if (xeFind == null) return null;
            }

            if ((xeFind == xDoc.Root) || (xeFind == null))
                return null;
            else
                return xeFind;
        }

        private XAttribute findAttribute(XElement xElement, string attrName)
        {
            if ((xElement == null) || string.IsNullOrEmpty(attrName)) return null;

            XAttribute[] attrs = xElement.Attributes(attrName).ToArray();
            if (attrs.Length == 0)
                return null;
            else
                return attrs[0];
        }

        private int findAttributeIndex(XElement xElement, string attrName)
        {
            if ((xElement == null) || string.IsNullOrEmpty(attrName)) return -1;

            XAttribute[] attrs = xElement.Attributes().ToArray();
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i].Name.LocalName == attrName) return i;
            }
            return -1;
        }

        private string getPathFromList(List<string> names)
        {
            return string.Join("/", names);
        }

        private List<string> getNamesFromXElement(XElement xElement)
        {
            List<string> retVal = new List<string>();
            retVal.Add(xElement.Name.LocalName);
            while (xElement.Parent != null)
            {
                xElement = xElement.Parent;
                retVal.Insert(0, xElement.Name.LocalName);
            }
            return retVal;
        }


    }  // class

    public class XMLCompareChangeItem
    {
        private List<string> _names;

        public List<string> Names { get { return _names; } }

        public string AttrName { get; set; }
        public XMLCompareResultEnum Result { get; set; }
        public string Value { get; set; }

        public XMLCompareChangeItem()
        {
            _names = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder bld = new StringBuilder();

            bld.Append($"element=\"{string.Join("/", _names)}\"");

            if (string.IsNullOrEmpty(this.AttrName) == false) bld.Append($" attr=\"{this.AttrName}\"");

            bld.Append($" action=\"{this.Result.ToString()}\"");

            if (this.Result == XMLCompareResultEnum.ChangeValue) bld.Append($" value=\"{this.Value}\"");

            return bld.ToString();
        }

        internal void SetNamesFromXElement(XElement curEl)
        {
            _names.Clear();

            _names.Add(curEl.Name.LocalName);
            while (curEl.Parent != null)
            {
                curEl = curEl.Parent;
                _names.Insert(0, curEl.Name.LocalName);
            }
        }
    }

    public enum XMLCompareResultEnum
    {
        AddNew, Remove, ChangeValue
    }

}
