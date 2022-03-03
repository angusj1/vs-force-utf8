using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Text;

namespace VILICVANE.ForceUTF8
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
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(ForceUTF8Package.PackageGuidString)]
    public sealed class ForceUTF8Package : Package
    {
        /// <summary>
        /// ForceUTF8Package GUID string.
        /// </summary>
        public const string PackageGuidString = "c0b70c8f-3e21-4e27-a874-799ffae09b48";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ForceUTF8Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        private DocumentEvents documentEvents;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            var dte = GetService(typeof(DTE)) as DTE2;
            documentEvents = dte.Events.DocumentEvents;
            documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;
        }

        void DocumentEvents_DocumentSaved(Document document)
        {
            if (document.Kind != "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}")
            {
                // then it's not a text file
                return;
            }

            var path = document.FullName;

            try
            {
                var stream = new FileStream(path, FileMode.Open);
                var reader = new StreamReader(stream, Encoding.Default, true);
                reader.Read();

                var preambleBytes = reader.CurrentEncoding.GetPreamble();
                if (preambleBytes.Length == 0 &&
                    reader.CurrentEncoding.EncodingName == Encoding.UTF8.EncodingName)
                {
                    stream.Close();
                    return;
                }

                string text;

                try
                {
                    stream.Position = 0;
                    reader = new StreamReader(stream, new UTF8Encoding(false, true));
                    text = reader.ReadToEnd();
                    stream.Close();
                    File.WriteAllText(path, text, new UTF8Encoding(false));
                    Output.Info($"Already convert file '{path}' encoding to utf-8 no BOM.");
                }
                catch (DecoderFallbackException)
                {
                    stream.Position = 0;
                    reader = new StreamReader(stream, Encoding.Default);
                    text = reader.ReadToEnd();
                    stream.Close();
                    File.WriteAllText(path, text, new UTF8Encoding(false));
                    Output.Info($"Already convert file '{path}' encoding to utf-8 no BOM.");
                }

            }
            catch (Exception e)
            {
                Output.Error($"Exception occured when converting file '{path}' encoding to utf-8 no BOM:\n{e.ToString()}");
            }
        }
        #endregion

    }
}
