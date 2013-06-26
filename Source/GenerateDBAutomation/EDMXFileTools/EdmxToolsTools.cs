
namespace EDMXFileTools
{
    using Microsoft.Data.Entity.Design.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Design;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// 
    /// </summary>
    public class EdmxTools
    {
        private readonly NamespaceManager _namespaceManager = null;

        public EdmxTools()
        {
            _namespaceManager = new NamespaceManager();
        }

        public static Boolean EdmxAutomationEnabled { get; private set; }
        public static Boolean RefreshOnSaveEnabled { get; private set; }

        public void GetProperties(ModelTransformExtensionContext context)
        {
            var edmxDoc = context.CurrentDocument;

            //Check for existance and enabled. The property is stored in the Conceptual section.
            var property = edmxDoc.Descendants(XName.Get("EdmxAutomationEnabled", "http://schemas.tempuri.com/EdmxAutomationEnabledDesignerExtension")).FirstOrDefault();
            if (property == null)
            {
                //If not present, we default to false;
                EdmxAutomationEnabled = false;
            }
            else
            {
                //If present and false we accept incoming EDMX..
                EdmxAutomationEnabled = Convert.ToBoolean(property.Value);
            }

            //Check for existance and enabled. The property is stored in the Conceptual section.
            property = edmxDoc.Descendants(XName.Get("RefreshOnSaveEnabled", "http://schemas.tempuri.com/RefreshOnSaveEnabledDesignerExtension")).FirstOrDefault();
            if (property == null)
            {
                //If not present, we default to false;
                RefreshOnSaveEnabled = false;
            }
            else
            {
                //If present and false we accept incoming EDMX..
                RefreshOnSaveEnabled = Convert.ToBoolean(property.Value);
            }
        }

        public void ReGenerateSsdlMslAndDdl(ModelTransformExtensionContext context)
        {
            var path = context.ProjectItem.FileNames[0];

            //The version of the Entity Framework loaded in PC.
            //The EF Designer offers an upgrade if the version of the EDMX is lower.
            var EfVersion = context.EntityFrameworkVersion;

            //This is the incoming EDMX document
            var edmxDoc = context.CurrentDocument;
            var version = _namespaceManager.GetVersionFromEDMXDocument(edmxDoc);

            var edmxns = _namespaceManager.GetEDMXNamespaceForVersion(version);
            var csns = _namespaceManager.GetCSDLNamespaceForVersion(version);
            var ssns = _namespaceManager.GetSSDLNamespaceForVersion(version);
            var msns = _namespaceManager.GetMSLNamespaceForVersion(version);

            XElement CsdlSchema = edmxDoc.Descendants(XName.Get("Schema", csns.NamespaceName)).FirstOrDefault();
            XElement Ssdl = edmxDoc.Descendants(XName.Get("StorageModels", edmxns.NamespaceName)).FirstOrDefault();
            XElement SsdlSchema = edmxDoc.Descendants(XName.Get("Schema", ssns.NamespaceName)).FirstOrDefault();
            XElement MslSchema = edmxDoc.Descendants(XName.Get("Mapping", msns.NamespaceName)).FirstOrDefault();

            XmlReader[] cReaders = { CsdlSchema.CreateReader() };
            IList<EdmSchemaError> cErrors = null;
            EdmItemCollection edmItemCollection = MetadataItemCollectionFactory.CreateEdmItemCollection(cReaders, out cErrors);

            String ssdlOut = String.Empty;
            String mslOut = String.Empty;
            List<Exception> errors = new List<Exception>();

            //Input parameter must be an EdmItemCollection based of the Schema part.
            if (RunCsdlToSsdlAndMslActivity.Generate(edmItemCollection, out ssdlOut, out mslOut, errors))
                return;

            var XssdlOut = XElement.Parse(ssdlOut);
            var XmslOut = XElement.Parse(mslOut);

            SsdlSchema.ReplaceWith(XssdlOut);
            MslSchema.ReplaceWith(XmslOut);

            String dslOut = String.Empty;

            //Input parameter must be a String based of the Schema part of the CsdlToSsdlAndMslActivity output,
            //whereas for Drop statements to be generated, the ExistingSsdl must be provided also based on the 
            //Schema section.
            //if (RunSsdlToDslActivity.Generate(XssdlOut.ToString(), SsdlSchema.ToString(), path, out dslOut, errors))
            //{
            //    return;
            //}

            //var sql = dslOut;
        }

        /// <summary>
        /// Stores the four sections from single edmx model into separate files.
        /// </summary>
        /// <param name="edmxDoc"></param>
        /// <param name="path"></param>
        public void StoreEdmxModel(ModelTransformExtensionContext context)
        {
            try
            {
                var path = context.ProjectItem.FileNames[0];

                //The Edmx document always exists and [hopefully] contains the designer surface property that
                //we use to determine whether EDMXFileTools is enabled.
                //The User may have set to true in this session to be saved.
                var edmxDoc = context.CurrentDocument;

                //Update properties as they may have been changed in this session.
                GetProperties(context);

                if (EdmxAutomationEnabled)
                {
                    //Re-create the Ssdl and Msl to ensure everything is Mapped.
                    //Entity Designer does not support custom mappings
                    ReGenerateSsdlMslAndDdl(context);
                }
            }
            catch (Exception ex)
            {
                var e = new ExtensionError(ex.Message, ex.HResult, ExtensionErrorSeverity.Message);
                context.Errors.Add(e);
            }
        }
    }

    /// <summary>
    /// Provides Namespace for each of the EDMX versions.
    /// From EdmGen2, although the codeplex source of EF itself
    /// has a very simmilar class.
    /// </summary>
    public class NamespaceManager
    {
        private static Version v1 = EntityFrameworkVersions.Version1;
        private static Version v2 = EntityFrameworkVersions.Version2;
        private static Version v3 = EntityFrameworkVersions.Version3;

        private Dictionary<Version, XNamespace> _versionToCSDLNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("http://schemas.microsoft.com/ado/2006/04/edm") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm") },
        { v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm") }
        };

        private Dictionary<Version, XNamespace> _versionToSSDLNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("http://schemas.microsoft.com/ado/2006/04/edm/ssdl") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/ssdl") },
        { v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl") } 
        };

        private Dictionary<Version, XNamespace> _versionToMSLNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("urn:schemas-microsoft-com:windows:storage:mapping:CS") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/mapping/cs") },
        { v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs") }
        };

        private Dictionary<Version, XNamespace> _versionToEDMXNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("http://schemas.microsoft.com/ado/2007/06/edmx") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/10/edmx") },
        { v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx") }
        };

        private Dictionary<XNamespace, Version> _namespaceToVersion = new Dictionary<XNamespace, Version>();

        internal NamespaceManager()
        {
            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToCSDLNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToSSDLNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToMSLNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToEDMXNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }
        }

        internal Version GetVersionFromEDMXDocument(XDocument xdoc)
        {
            XElement el = xdoc.Root;
            if (el.Name.LocalName.Equals("Edmx") == false)
            {
                throw new ArgumentException("Unexpected root node local name for edmx document");
            }
            return this.GetVersionForNamespace(el.Name.Namespace);
        }

        internal Version GetVersionFromCSDLDocument(XDocument xdoc)
        {
            XElement el = xdoc.Root;
            if (el.Name.LocalName.Equals("Schema") == false)
            {
                throw new ArgumentException("Unexpected root node local name for csdl document");
            }
            return this.GetVersionForNamespace(el.Name.Namespace);
        }

        internal XNamespace GetMSLNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToMSLNamespace.TryGetValue(v, out n);
            return n;
        }

        internal XNamespace GetCSDLNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToCSDLNamespace.TryGetValue(v, out n);
            return n;
        }

        internal XNamespace GetSSDLNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToSSDLNamespace.TryGetValue(v, out n);
            return n;
        }

        internal XNamespace GetEDMXNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToEDMXNamespace.TryGetValue(v, out n);
            return n;
        }

        //Designer namespace is same as EDMX
        internal XNamespace GetDESNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToEDMXNamespace.TryGetValue(v, out n);
            return n;
        }

        internal Version GetVersionForNamespace(XNamespace n)
        {
            Version v;
            _namespaceToVersion.TryGetValue(n, out v);
            return v;
        }
    }
}
