
namespace MultiFileEDMX.Properties
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// This class has one public property, MultiFileEDMXEnabledProperty. 
    /// This property is visible in the Visual Studio Properties window when the
    /// Designer Surface is selected.
    /// This property and its value are saved as a structured annotation in
    /// the conceptual content of an .edmx document.
    /// </summary>
    class MultiFileEDMXEnabledProperty
    {
        internal static readonly string _namespace = "http://schemas.tempuri.com/MultiFileEDMXEnabledProperty";
        internal static XName _xnMyNamespace = XName.Get("MultiFileEDMXEnabledProperty", _namespace);
        internal const string _category = "Multifile EDMX Tools";

        private XElement _parent;
        private PropertyExtensionContext _context;

        public MultiFileEDMXEnabledProperty(XElement parent, PropertyExtensionContext context)
        {
            _context = context;
            _parent = parent;
        }

        // This property is saved in the conceptual content of an .edmx document in the following format:
        // <EntityType>
        //  <!-- other entity properties -->
        //  <MyNewProperty xmlns="http://schemas.tempuri.com/MyNewProperty">True</MyNewProperty>
        // </EntityType>
        [DisplayName("MultiFileTools Status")]
        [Description("Sets the status of the MultiFileTools: True to save in multifile format.")]
        [Category(MultiFileEDMXEnabledProperty._category)]
        [DefaultValue(false)]
        public bool MultiFileEnabled
        {
            // Read and return the property value from the structured annotation in the EntityType element.
            get
            {
                bool propertyValue = false;
                if (_parent.HasElements)
                {
                    XElement lastChild = _parent.Elements().Where<XElement>(element => element != null && element.Name == MultiFileStatusProperty._xnMyNamespace).LastOrDefault();
                    if (lastChild != null)
                    {
                        // Property element exists, so get its value.
                        bool boolValue = false;
                        if (Boolean.TryParse(lastChild.Value.Trim(), out boolValue))
                        {
                            propertyValue = boolValue;
                        }
                    }
                }
                return propertyValue;
            }

            // Write the new property value to the structured annotation in the EntityType element.
            set
            {
                bool propertyValue = value;

                // Make changes to the .edmx document in an EntityDesignerChangeScope to enable undo/redo of changes.
                using (EntityDesignerChangeScope scope = _context.CreateChangeScope("Set MultiFileStatusProperty"))
                {
                    if (_parent.HasElements)
                    {
                        XElement lastChild = _parent.Elements().Where<XElement>(element => element != null && element.Name == MultiFileStatusProperty._xnMyNamespace).LastOrDefault();
                        if (lastChild != null)
                        {
                            // Property element already exists under the EntityType element, so update its value.
                            lastChild.SetValue(propertyValue.ToString());
                        }
                        else
                        {
                            // Property element does not exist, so create a new one as the last child of the EntityType element.
                            _parent.Elements().Last().AddAfterSelf(new XElement(_xnMyNamespace, propertyValue.ToString()));
                        }
                    }
                    else
                    {
                        // The element has no child elements so create a new MyNewProperty element as its first child.
                        _parent.Add(new XElement(_xnMyNamespace, propertyValue.ToString()));
                    }

                    // Commit the changes.
                    scope.Complete();
                }
            }
        }
    }
}
