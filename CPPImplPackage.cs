using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.IO;
using mx.GenImpl;
using System.Windows.Forms;
using EnvDTE80;

namespace mx.CPPImpl
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCPPImplPkgString)]
    public sealed class CPPImplPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CPPImplPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
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
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidCPPImplCmdSet, (int)PkgCmdIDList.GenImpl);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand(menuItem);

                CommandID menuCommandID1 = new CommandID(GuidList.guidCPPImplCmdSet, (int)PkgCmdIDList.GenImplFunc);
                MenuCommand menuItem1 = new MenuCommand(MenuItemCallbackFunc, menuCommandID1 );
                mcs.AddCommand( menuItem1 );
            }
        }
        #endregion
        private void ShowStandardIncludeDirectories(EnvDTE.Project project)
        {
            Microsoft.VisualStudio.VCProjectEngine.VCProject proj;
            Microsoft.VisualStudio.VCProjectEngine.VCPlatform platform;
            Microsoft.VisualStudio.VCProjectEngine.IVCCollection configurationsCollection;

            proj = (Microsoft.VisualStudio.VCProjectEngine.VCProject)project.Object;

            configurationsCollection = (Microsoft.VisualStudio.VCProjectEngine.IVCCollection)proj.Configurations;

            foreach (Microsoft.VisualStudio.VCProjectEngine.VCConfiguration configuration in configurationsCollection)
            {
                platform = (Microsoft.VisualStudio.VCProjectEngine.VCPlatform)configuration.Platform;

                MessageBox.Show(configuration.Name + ": " + platform.IncludeDirectories);
            }
        }
        private int msgBox(string text, OLEMSGICON icon, OLEMSGBUTTON buttons)
        {
            int result;
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Generate Implementation:",
                       text,
                       string.Empty,
                       0,
                       buttons,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       icon,
                       0,        // false
                       out result));
            return result;
        }
        private void MenuItemCallbackFunc(object sender, EventArgs e)
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(SDTE));
            TextSelection objSel = (TextSelection)dte.ActiveDocument.Selection;
            string file = dte.ActiveDocument.FullName;
            if (file.ToLower().EndsWith(".h") ||
                file.ToLower().EndsWith(".hpp") ||
                file.ToLower().EndsWith(".hh") ||
                file.ToLower().EndsWith(".hxx"))
            {
                string filecpp = Path.ChangeExtension(file, "cpp");
                if (System.IO.File.Exists(filecpp))
                {
                    System.IO.File.AppendAllText(filecpp, Parse.getFunction(file, (uint)objSel.ActivePoint.Line, (uint)objSel.ActivePoint.DisplayColumn));
                }
                else
                {
                    System.IO.File.WriteAllText(filecpp, Parse.getFunction(file, (uint)objSel.ActivePoint.Line, (uint)objSel.ActivePoint.DisplayColumn));

                    dte.ItemOperations.AddExistingItem(filecpp);
                }

            }
            else
            {
                msgBox("Not a header file", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }




        }
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(SDTE));
            TextSelection objSel = (TextSelection)dte.ActiveDocument.Selection;

            string file = dte.ActiveDocument.FullName;
            if (file.ToLower().EndsWith(".h") ||
                file.ToLower().EndsWith(".hpp") ||
                file.ToLower().EndsWith(".hh") ||
                file.ToLower().EndsWith(".hxx"))
            {
                string filecpp = Path.ChangeExtension(file, "cpp");
                if (System.IO.File.Exists(filecpp))
                {
                    int res = msgBox("File exists overwrite?", OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNO);
                    if (res == 6) //IDYES
                    {
                        System.IO.File.WriteAllText(filecpp, Parse.getCPP(file));
                        //dte.ItemOperations.AddExistingItem(filecpp);
                    }
                }
                else
                {
                    System.IO.File.WriteAllText(filecpp, Parse.getCPP(file));
                    dte.ItemOperations.AddExistingItem(filecpp);
                }

            }
            else
            {
                msgBox("Not a header file", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }

        }

    }
}
