using System;
using System.Collections.Generic;
using System.Linq;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    public sealed partial class RemoveUnwantedFilesWindow
    {
        #region Constructors

        public RemoveUnwantedFilesWindow()
        {
            InitializeComponent();

            ViewModel.Closed += (sender, args) => DialogResult = true;

            ViewModel.ActionCompleted +=
                (sender, args) =>
                {
                    var currentItem = ViewModel.LogRecords.CurrentItem;
                    if (currentItem != null)
                    {
                        LogDataGrid.ScrollIntoView(currentItem);
                    }
                };
        }

        #endregion
    }
}