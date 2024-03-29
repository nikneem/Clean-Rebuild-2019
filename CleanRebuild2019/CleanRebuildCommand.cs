﻿using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Threading = System.Threading.Tasks;

namespace CleanRebuild2019
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CleanRebuildCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3853fd95-211a-49e4-add4-4106d3826b86");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private DTE _dte;
        private MenuCommand _command;


        /// <summary>
        /// Initializes a new instance of the <see cref="CleanRebuildCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CleanRebuildCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            _command = new MenuCommand(this.Execute, menuCommandID)
            {
                Enabled = false
            };
            commandService.AddCommand(_command);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CleanRebuildCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Threading.Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CleanRebuildCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CleanRebuildCommand(package, commandService);
            Instance._dte = await Instance.GetServiceByTypeAsync<DTE>();
            //Instance._dte.Events.SolutionEvents.Opened += Instance.SolutionOpenendEventHandler;
            //Instance._dte.Events.SolutionEvents.BeforeClosing += Instance.SolutionClosingEventHandler;
        }


        //private void SolutionOpenendEventHandler()
        //{
        //    _command.Enabled = true;
        //}
        //private void SolutionClosingEventHandler()
        //{
        //    _command.Enabled = false;
        //}




        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            _command.Enabled = false;
            if (_dte.Solution != null && _dte.Solution.Count > 0)
            {
                _dte.Solution.SolutionBuild.Clean(true);
                _dte.Solution.SolutionBuild.Build();
            }
            _command.Enabled = true;
        }


        /// <summary>
        /// Represents the method that
        /// retrieve the service with
        /// the passed type.
        /// </summary>
        public async Task<T> GetServiceByTypeAsync<T>() where T : class
        {
            return await package.GetServiceAsync(typeof(T)) as T;
        }
    }
}
