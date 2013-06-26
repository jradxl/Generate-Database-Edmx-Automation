using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MyCompany.RdtEventExplorer;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Company.VSPackageCM
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0.2", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(RdtEventWindowPane))]
    [ProvideOptionPage(typeof(RdtEventOptionsDialog), "RDT Event Explorer ZZ", "Explorer Options", 0, 0, true)]
    //Force a load of this package instead of on demand
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists)]
    [Guid(GuidList.guidVSPackageGDBAPkgString)]
    public sealed class GenDBAutomationPackage : Package,
        IVsRunningDocTableEvents, IVsRunningDocTableEvents4 //, IVsRunningDocTableEvents2, IVsRunningDocTableEvents3, 
    {
        // Cache the Menu Command Service since we will use it multiple times
        private OleMenuCommandService menuService;
        const int bitmapResourceID = 300;

        //Edmx saving managment
        uint rdtCookie;
        RunningDocumentTable rdt;
        //ProjectItem projectItem = null;
        String closingDocument = String.Empty;
        Microsoft.VisualStudio.Shell.SelectionContainer selectionContainer;

        public GenDBAutomationPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(RdtEventWindowPane), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        //Used by the Refresh Context Menu
        private void RefreshCommandCallBack(object sender, EventArgs e)
        {
            DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            ProjectItem pi = null;
            Array projects = (Array)dte.ActiveSolutionProjects;
            Project activeProject = (Project)projects.GetValue(0);

            //ActiveDocument can only be the .Edmx file in the Designer.
            //Locate the Projectitem reference for this using full path and filename as Key.
            foreach (ProjectItem proj in activeProject.ProjectItems)
            {
                pi = proj;
                if (proj.FileNames[0] == dte.ActiveDocument.FullName)
                    break;
            }

            //Close and immediately re-Open the .Edmx file to
            //force a refresh of the Model in the Designer.
            dte.ActiveDocument.Close();
            pi.Open().Activate();
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the Refresh Command and its handler
                CommandID refreshCommandID = new CommandID(GuidList.guidVSPackageGDBACmdSet, (int)PkgCmdIDList.cmdidRefreshContextMenu);
                MenuCommand menuRefresh = new MenuCommand(RefreshCommandCallBack, refreshCommandID);
                mcs.AddCommand(menuRefresh);

                // Create the command for the tool window for the RDT Event List
                CommandID toolwndCommandID = new CommandID(GuidList.guidRdtEventExplorerCmdSet, (int)PkgCmdIDList.cmdidMyTool);
                MenuCommand menuToolWin = new MenuCommand(new EventHandler(ShowToolWindow), toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }

            //Alternative method
            //uint dwCookie = 0;
            //IVsRunningDocumentTable rdt2 = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));
            //rdt2.AdviseRunningDocTableEvents(this, out dwCookie);

            // Create a selection container for tracking selected RDT events.
            selectionContainer = new Microsoft.VisualStudio.Shell.SelectionContainer();

            // Advise the RDT of this event sink.
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp = Package.GetGlobalService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider)) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
            if (sp == null) return;

            rdt = new RunningDocumentTable(new ServiceProvider(sp));
            if (rdt == null) return;

            rdtCookie = rdt.Advise(this);
        }

        /// <summary>
        /// Define a command handler for the RDT Event Tool Window
        /// When the user press the button corresponding to the CommandID
        /// the EventHandler will be called.
        /// </summary>
        /// <param name="id">The CommandID (Guid/ID pair) as defined in the .vsct file</param>
        /// <param name="handler">Method that should be called to implement the command</param>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        internal OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            // if the package is zombied, we don't want to add commands
            if (this.Zombied)
                return null;

            // Make sure we have the service
            if (menuService == null)
            {
                // Get the OleCommandService object provided by the MPF; this object is the one
                // responsible for handling the collection of commands implemented by the package.
                menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            }
            OleMenuCommand command = null;
            if (null != menuService)
            {
                // Add the command handler
                command = new OleMenuCommand(handler, id);
                menuService.AddCommand(command);
            }
            return command;
        }
        #endregion

        public int OnAfterSave(uint docCookie)
        {
            if (EDMXFileTools.EdmxTools.RefreshOnSaveEnabled)
            {
                var ev = new GenericEvent(rdt, "OnAfterSave", docCookie);
                var document = ev.DocumentName;
                var extension = System.IO.Path.GetExtension(document);
                //We'll make assumuption that the extension won't change.
                if (extension.Equals(".edmx"))
                {
                    DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;

                    Array projects = (Array)dte.ActiveSolutionProjects;
                    Project activeProject = (Project)projects.GetValue(0);

                    //ActiveDocument can only be the .Edmx file in the Designer.
                    //Locate the Projectitem reference for this using full path and filename as Key.
                    foreach (ProjectItem proj in activeProject.ProjectItems)
                    {
                        foreach (Document doc in dte.Documents)
                        {
                            if (doc.FullName.Equals(ev.DocumentMoniker))
                            {
                                //projectItem = proj;
                                closingDocument = ev.DocumentMoniker;
                                doc.Close();
                                return VSConstants.S_OK;
                            }
                        }
                    }
                }
            }
            return VSConstants.S_OK;
        }

        public int OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument, int fClosedWithoutSaving)
        {
            //The close initiated in OnAfterSave will cause an additional call here for the .diagram file.
            if (EDMXFileTools.EdmxTools.RefreshOnSaveEnabled && !String.IsNullOrEmpty(closingDocument)) //&& projectItem != null)
            {
                if (pszMkDocument.Equals(closingDocument))
                {
                    try
                    {
                        DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                        Array projects = (Array)dte.ActiveSolutionProjects;
                        Project activeProject = (Project)projects.GetValue(0);
                        foreach (ProjectItem item in activeProject.ProjectItems)
                        {
                            if (item.FileNames[0].Equals(closingDocument))
                            {
                                if (!item.IsOpen)
                                    item.Open().Activate();
                                closingDocument = String.Empty;
                                //projectItem = null;
                                return VSConstants.S_OK;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("EdmxFileTools Error reactivating {0}, {1}", closingDocument, ex.Message));
                    }
                }
            }
            return VSConstants.S_OK;
        }

        #region Unused Callbacks
        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSaveAll()
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument)
        {
            return VSConstants.S_OK;
        }
        #endregion
    }
}
