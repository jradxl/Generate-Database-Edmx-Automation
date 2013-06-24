
namespace MultiFileEDMXTools.Properties
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using System;
    using System.Data.Metadata.Edm;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    class PropertyManager
    {
        public static string GetValueAsString(XElement _parent, XName xName)
        {
            string result = string.Empty;
            if (_parent.HasElements)
            {
                XElement lastChild = _parent.Elements().Where<XElement>(element => element != null && element.Name == xName).LastOrDefault();
                if (lastChild != null)
                    // Property element exists, so get its value.
                    result = lastChild.Value.ToString();
            }
            return result;
        }

        public static bool GetValueAsBool(XElement _parent, XName xName)
        {
            bool result = false;
            if (_parent.HasElements)
            {
                XElement lastChild = _parent.Elements().Where<XElement>(element => element != null && element.Name == xName).LastOrDefault();
                if (lastChild != null)
                {
                    // Property element exists, so get its value.
                    bool boolValue = false;
                    if (Boolean.TryParse(lastChild.Value.Trim(), out boolValue))
                    {
                        result = boolValue;
                    }
                }
            }
            return result;
        }

        public static void SetValue(XElement _parent, PropertyExtensionContext _context, XName xName, string value)
        {
            string propertyValue;
            if (value != null)
                propertyValue = value.Trim();
            else
                propertyValue = string.Empty;

            using (EntityDesignerChangeScope scope = _context.CreateChangeScope("Set MultiFileEDMXTools"))
            {
                if (_parent.HasElements)
                {
                    XElement lastChild = _parent.Elements().Where<XElement>(element => element != null && element.Name == xName).LastOrDefault();
                    if (lastChild != null)
                    {
                        lastChild.SetValue(propertyValue);
                    }
                    else
                    {
                        // MyNewProperty element does not exist, so create a new one as the last 
                        // child of the EntityType element.
                        _parent.Elements().Last().AddAfterSelf(new XElement(xName, propertyValue));
                    }
                }
                else
                {
                    // The EntityType element has no child elements so create a new MyNewProperty 
                    // element as its first child.
                    _parent.Add(new XElement(xName, propertyValue));
                }

                // Commit the changes.
                scope.Complete();
            }
        }

        public static void SetValue(XElement _parent, PropertyExtensionContext _context, XName xName, bool value)
        {
            bool propertyValue = value;

            // Make changes to the .edmx document in an EntityDesignerChangeScope to enable undo/redo of changes.
            using (EntityDesignerChangeScope scope = _context.CreateChangeScope("Set MultiFileEDMXTools"))
            {
                if (_parent.HasElements)
                {
                    XElement lastChild = _parent.Elements().Where<XElement>(element => element != null && element.Name == xName).LastOrDefault();
                    if (lastChild != null)
                    {
                        // Property element already exists under the EntityType element, so update its value.
                        lastChild.SetValue(propertyValue.ToString());
                    }
                    else
                    {
                        // Property element does not exist, so create a new one as the last child of the EntityType element.
                        _parent.Elements().Last().AddAfterSelf(new XElement(xName, propertyValue.ToString()));
                    }
                }
                else
                {
                    // The element has no child elements so create a new MyNewProperty element as its first child.
                    _parent.Add(new XElement(xName, propertyValue.ToString()));
                }

                // Commit the changes.
                scope.Complete();
            }
        }
    }
}