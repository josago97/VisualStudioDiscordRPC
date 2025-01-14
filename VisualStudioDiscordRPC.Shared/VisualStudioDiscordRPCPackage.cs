﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE80;
using VisualStudioDiscordRPC.Shared.Commands;
using Task = System.Threading.Tasks.Task;

namespace VisualStudioDiscordRPC.Shared
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VisualStudioDiscordRPCPackage : AsyncPackage
    {
        /// <summary>
        /// VisualStudioDiscordRPCPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "5cd3d640-3d33-45ea-8c5b-6de981ff9900";
        public PackageController Controller { get; private set; }

        #region Package Members

        private static string GetAssemblyLocalPathFrom(Type type)
        {
            string codebase = type.Assembly.Location;
            var uri = new Uri(codebase, UriKind.Absolute);

            return Path.GetDirectoryName(uri.LocalPath);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            string installationPath = GetAssemblyLocalPathFrom(typeof(VisualStudioDiscordRPCPackage));
            
            // DTE settings
            var instance = (DTE2) GetService(typeof(DTE));
            if (instance == null)
            {
                throw new InvalidOperationException("Can not get DTE Service");
            }

            Controller = new PackageController(instance, installationPath);
            await SettingsCommand.InitializeAsync(this);
        }

        protected override int QueryClose(out bool canClose)
        {
            Controller.Dispose();
            return base.QueryClose(out canClose);
        }

        #endregion
    }
}
