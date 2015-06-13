using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ICSharpCode.Core;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.Commands
{
    public sealed class RemoveUnwantedFilesCommand : AbstractMenuCommand
    {
        #region Public Methods

        /// <summary>
        ///     Invokes the command.
        /// </summary>
        public override void Run()
        {
            MessageBox.Show("Here will open the 'Remove unwanted files' dialog...");

            //// TODO [vmcl] Implement RemoveUnwantedFilesCommand.Run
        }

        #endregion
    }
}