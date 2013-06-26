using Microsoft.Data.Entity.Design.Extensibility;
using System.ComponentModel.Composition;

namespace EDMXFileTools
{
    [Export(typeof(IModelTransformExtension))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ModelTransformExtension : IModelTransformExtension
    {
        /// <summary>
        /// Called after the .edmx document has been loaded, but before the contents are displayed in the
        /// Entity Designer.
        /// 
        /// Construct an EDMX model by loading storage, conceptual, and mapping model from separate files and moerging them 
        /// into the EDMX model.
        /// </summary>
        /// <param name="context"></param>
        void IModelTransformExtension.OnAfterModelLoaded(ModelTransformExtensionContext context)
        {
            //AfterModelLoaded only used to access Designer Surface Properties
            new EdmxTools().GetProperties(context);
        }

        /// <summary>
        /// Called immediately before the .edmx document is saved.
        /// 
        /// Save storage, conceptual, and mapping models in separate files and reset the content of the EDMX model.
        /// </summary>
        /// <param name="context"></param>
        void IModelTransformExtension.OnBeforeModelSaved(ModelTransformExtensionContext context)
        {
            //Now carryout the Save option, with option recreation of the Ssdl and Msl.
            new EdmxTools().StoreEdmxModel(context);
        }
    }
}
