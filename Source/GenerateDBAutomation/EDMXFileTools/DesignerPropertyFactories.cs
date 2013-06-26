
namespace EDMXFileTools.Properties
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using System.ComponentModel.Composition;
    using System.Xml.Linq;

    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IEntityDesignerExtendedProperty))]
    [EntityDesignerExtendedProperty(EntityDesignerSelection.DesignerSurface)]
    class EDMXFileToolsEnabledPropertyFactory : IEntityDesignerExtendedProperty
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object CreateProperty(XElement element, PropertyExtensionContext context)
        {
            return new EdmxAutomationEnabledProperty(element, context);
        }
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IEntityDesignerExtendedProperty))]
    [EntityDesignerExtendedProperty(EntityDesignerSelection.DesignerSurface)]
    class RefreshOnSaveEnabledPropertyFactory : IEntityDesignerExtendedProperty
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object CreateProperty(XElement element, PropertyExtensionContext context)
        {
            return new RefreshOnSaveEnabledProperty(element, context);
        }
    }
}
