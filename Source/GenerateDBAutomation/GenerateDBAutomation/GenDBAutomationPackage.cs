using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Company.VSPackageCM
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0.1", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidVSPackageGDBAPkgString)]
    public sealed class GenDBAutomationPackage : Package
    {
        public GenDBAutomationPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

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
                // Create the Refresh Command and it's handler
                CommandID refreshCommandID = new CommandID(GuidList.guidVSPackageGDBACmdSet, (int)PkgCmdIDList.cmdidRefreshContextMenu);
                MenuCommand menuRefresh = new MenuCommand(RefreshCommandCallBack, refreshCommandID);
                mcs.AddCommand(menuRefresh);
            }

        }
        #endregion
    }
}
