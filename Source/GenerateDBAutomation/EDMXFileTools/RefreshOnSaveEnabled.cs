
namespace EDMXFileTools.Properties
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using System.ComponentModel;
    using System.Xml.Linq;

    class RefreshOnSaveEnabledProperty
    {
        private XElement _parent;
        private PropertyExtensionContext _context;

        private const string _designerExtensionsNamespace = @"http://schemas.tempuri.com/RefreshOnSaveEnabledDesignerExtension";
        static readonly XName _xnStarterKitNamespace = XName.Get("RefreshOnSaveEnabled", _designerExtensionsNamespace);

        public RefreshOnSaveEnabledProperty(XElement parent, PropertyExtensionContext context)
        {
            _context = context;
            _parent = parent;
        }

        [DisplayName("Refresh On Save Enabled")]
        [Description("Set to True to enable refresh of Designer on save.")]
        [Category("EDMX File Tools")]
        public bool RefreshOnSaveEnabled
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
