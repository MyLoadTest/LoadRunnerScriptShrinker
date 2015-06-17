using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    public sealed partial class RemoveUnwantedFilesWindow
    {
        #region Constructors

        public RemoveUnwantedFilesWindow()
        {
            InitializeComponent();

            ViewModel.ActionExecuted += ViewModel_ActionExecuted;
        }

        #endregion

        #region Private Methods

        private void ViewModel_ActionExecuted(bool logicExecuted, long totalSize, Exception exception)
        {
            string message = null;
            var messageBoxImage = MessageBoxImage.Information;

            if (exception == null)
            {
                if (logicExecuted)
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The selected files have been removed (size: {0}).",
                        totalSize.FormatFileSize());
                }
            }
            else
            {
                messageBoxImage = MessageBoxImage.Error;
                message = string.Format(CultureInfo.InvariantCulture, "Error occurred: {0}", exception.Message);
            }

            if (!message.IsNullOrEmpty())
            {
                MessageBox.Show(this, message, Title, MessageBoxButton.OK, messageBoxImage);
            }

            DialogResult = logicExecuted;
        }

        #endregion
    }
}