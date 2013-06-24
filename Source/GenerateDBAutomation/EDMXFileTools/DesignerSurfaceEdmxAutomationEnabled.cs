
namespace MultiFileEDMXTools.Properties
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using System.ComponentModel;
    using System.Xml.Linq;

    class EdmxAutomationEnabledProperty
    {
        private XElement _parent;
        private PropertyExtensionContext _context;

        private const string _designerExtensionsNamespace = @"http://schemas.tempuri.com/EdmxAutomationEnabledDesignerExtension";
        static readonly XName _xnStarterKitNamespace = XName.Get("EdmxAutomationEnabled", _designerExtensionsNamespace);

        public EdmxAutomationEnabledProperty(XElement parent, PropertyExtensionContext context)
        {
            _context = context;
            _parent = parent;
        }

        [DisplayName("Enabled")]
        [Description("Set to True to enable creation of new Ssdl and Msl.")]
        [Category("EDMX File Tools")]
        public bool MultiFileEDMXToolsEnabled
        {
            get { return getValue(_xnStarterKitNamespace); }
            set { setValue(_xnStarterKitNamespace, value); }
        }

        protected bool getValue(XName xName)
        {
            return PropertyManager.GetValueAsBool(_parent, xName);
        }

        protected void setValue(XName xName, bool value)
        {
            PropertyManager.SetValue(_parent, _context, xName, value);
        }
    }
}
