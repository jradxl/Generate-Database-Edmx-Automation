
namespace EDMXFileTools
{
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Activities;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators;
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Data.Entity.Design;
    using System.Data.Metadata.Edm;
    using System.Threading;

    public class RunSsdlToDslActivity
    {
        public static bool Generate(String Ssdl, String exSsdl, string edmxPath, out String ddl, List<Exception> errors)
        {
            String DdlOut = null;
            bool HasErrors = false;

            Version version = EntityFrameworkVersions.Version3;
            var inputs = new Dictionary<string, object>()
                                          {
                                           {"ExistingSsdlInput", exSsdl},
                                           {"SsdlInput", Ssdl }
                                          };

            var resolver = new SymbolResolver();
            var edmParameterBag = new EdmParameterBag(
                                                     null, // syncContext
                                                     null, // assemblyLoader
                                                     version, // targetVersion
                                                     "System.Data.SqlClient", // providerInvariantName
                                                     "2008", // providerManifestToken
                                                     "none", // providerConnectionString
                                                     "dbo", // databaseSchemaName
                                                     "Dummy", // databaseName
                //TODO - get ddlTemplatePath from registry?
                                                     @"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\Extensions\Microsoft\Entity Framework Tools\DBGen\SSDLToSQL10.tt", // ddlTemplatePath
                                                     edmxPath // edmxPath
                                                     );

            resolver.Add(typeof(EdmParameterBag).Name, edmParameterBag);

            SsdlToDdlActivity activity = new SsdlToDdlActivity();
            var wfa = new WorkflowApplication(activity, inputs);
            wfa.Extensions.Add(resolver);

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            AutoResetEvent idleEvent = new AutoResetEvent(false);

            wfa.Completed = delegate(WorkflowApplicationCompletedEventArgs e)
            {
                syncEvent.Set();
                DdlOut = (string)e.Outputs["DdlOutput"];
            };

            wfa.Aborted = e =>
            {
                syncEvent.Set();
                errors.Add(e.Reason);
                HasErrors = false;
            };

            wfa.OnUnhandledException = e =>
            {
                syncEvent.Set();
                errors.Add(e.UnhandledException);
                HasErrors = false;
                return UnhandledExceptionAction.Cancel;
            };

            wfa.Run();

            // Loop until the workflow completes.
            WaitHandle[] handles = new WaitHandle[] { syncEvent, idleEvent };
            while (WaitHandle.WaitAny(handles) != 0)
            {
            }

            ddl = DdlOut;
            return HasErrors;
        }
    }

    public class RunCsdlToSsdlAndMslActivity
    {
        public static bool Generate(EdmItemCollection csdl, out String ssdl, out String msl, List<Exception> errors)
        {
            String SsdlOut = null;
            String MslOut = null;

            bool HasErrors = false;

            var version = EntityFrameworkVersions.Version3;
            var inputs = new Dictionary<string, object>()
                                          {
                                           {"CsdlInput", csdl},
                                           {"OutputGeneratorType", typeof(CsdlToSsdl).AssemblyQualifiedName },
                                           {"MslOutputGeneratorType", typeof(CsdlToMsl).AssemblyQualifiedName }
                                          };

            var resolver = new SymbolResolver();
            var edmParameterBag = new EdmParameterBag(
                                                     null, // syncContext
                                                     null, // assemblyLoader
                                                     version, // targetVersion
                                                     "System.Data.SqlClient", // providerInvariantName
                                                     "2008", // providerManifestToken
                                                     null, // providerConnectionString
                                                     "dbo", // databaseSchemaName
                                                     null, // databaseName
                                                     null, // ddlTemplatePath
                                                     null // edmxPath
                                                     );
            resolver.Add(typeof(EdmParameterBag).Name, edmParameterBag);

            var activity = new CsdlToSsdlAndMslActivity();
            var wfa = new WorkflowApplication(activity, inputs);
            wfa.Extensions.Add(resolver);

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            AutoResetEvent idleEvent = new AutoResetEvent(false);

            wfa.Completed = delegate(WorkflowApplicationCompletedEventArgs e)
            {
                syncEvent.Set();
                SsdlOut = (string)e.Outputs["SsdlOutput"];
                MslOut = (string)e.Outputs["MslOutput"];
            };

            wfa.Aborted = e =>
            {
                syncEvent.Set();
                errors.Add(e.Reason);
                HasErrors = true;
            };

            wfa.OnUnhandledException = e =>
            {
                syncEvent.Set();
                errors.Add(e.UnhandledException);
                HasErrors = true;
                return UnhandledExceptionAction.Cancel;
            };

            wfa.Run();

            // Loop until the workflow completes.
            WaitHandle[] handles = new WaitHandle[] { syncEvent, idleEvent };
            while (WaitHandle.WaitAny(handles) != 0)
            {
            }

            ssdl = SsdlOut;
            msl = MslOut;
            return HasErrors;
        }

        /// <summary>
        /// From http://www.timvw.be/2007/01/08/generating-utf-8-with-systemxmlxmlwriter/
        /// http://www.undermyhat.org/blog/2009/08/tip-force-utf8-or-other-encoding-for-xmlwriter-with-stringbuilder/
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        //public static string ConvertUFT16toUFT8(String str)
        //{
        //MemoryStream memoryStream = new MemoryStream();
        //XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() { Encoding = new UTF8Encoding(true, true), ConformanceLevel = ConformanceLevel.Auto, Indent = true };
        //XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
        //xmlWriter.WriteString(str);
        //xmlWriter.WriteStartDocument();
        //xmlWriter.WriteStartElement("root", "http://www.timvw.be/ns");
        //xmlWriter.WriteEndElement();
        //xmlWriter.WriteEndDocument();
        //xmlWriter.Flush();
        //xmlWriter.Close();
        //string xmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
        //return xmlString;

        //XmlWriterSettings settings = new XmlWriterSettings();
        //settings.Encoding = Encoding.UTF8;

        //StringBuilder xmlAsUtf8 = new StringBuilder();
        //using (XmlWriter xmlWriter = XmlWriter.Create(xmlAsUtf8, settings))
        //{
        //    /*
        //        Set the correct encoding using the extension
        //        method of the UnderMyHat library.
        //        MUST BE THE FIRST LINE AFTER INSTANTIATION
        //    */
        //    xmlWriter.ForceEncoding(Encoding.UTF8);
        //    xmlWriter.WriteStartElement("encoding");
        //    xmlWriter.WriteAttributeString("should-be", "utf-8");
        //    xmlWriter.WriteEndElement();
        //}
        //}
        //var s = @"<?xml version=""1.0"" encoding=""utf-16""?>"; //escaped
        //byte[] ut16Bytes = Encoding.Unicode.GetBytes(SsdlOut);
        //byte[] buffer = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, ut16Bytes);
        //var ssdl1 = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        //XElement ssdlx = XElement.Parse(ssdl1);
        //ssdl = ssdlx.ToString();

        //ut16Bytes = Encoding.Unicode.GetBytes(MslOut);
        //buffer = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, ut16Bytes);
        //ssdl1 = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        //XElement b = XElement.Parse(ssdl1);
        //msl = b.ToString();

    }
}
