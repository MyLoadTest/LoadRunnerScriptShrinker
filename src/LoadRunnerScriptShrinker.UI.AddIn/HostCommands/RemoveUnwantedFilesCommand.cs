using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using HP.LR.VuGen.ServiceCore;
using HP.LR.VuGen.ServiceCore.Interfaces;
using ICSharpCode.Core;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.HostCommands
{
    public sealed class RemoveUnwantedFilesCommand : AbstractMenuCommand
    {
        #region Public Methods

        /// <summary>
        ///     Invokes the command.
        /// </summary>
        public override void Run()
        {
            var projectService = VuGenServiceManager.GetService<IVuGenProjectService>().EnsureNotNull();

            var script = projectService.GetActiveScript();
            if (script == null)
            {
                return;
            }

            var window = new RemoveUnwantedFilesWindow
            {
                Owner = Application.Current.Morph(app => app.MainWindow),
                ViewModel =
                {
                    ScriptPath = script.FileName
                }
            };

            window.ShowDialog();
        }

        #endregion
    }
}