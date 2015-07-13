using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.Commands;
using Omnifactotum.Annotations;
using ICommand = System.Windows.Input.ICommand;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    internal sealed class RemoveUnwantedFilesWindowViewModel : DependencyObject
    {
        #region Constants and Fields

        public static readonly DependencyProperty ScriptPathProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.ScriptPath,
                new PropertyMetadata(null, OnScriptPathChangedInternal));

        public static readonly DependencyProperty ShouldRemoveRecordingLogsProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.ShouldRemoveRecordingLogs,
                new PropertyMetadata(OnShouldRemoveRecordingLogsChangedInternal));

        public static readonly DependencyProperty RecordingLogSizeStringProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.RecordingLogSizeString);

        public static readonly DependencyProperty ShouldRemoveReplayLogsProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.ShouldRemoveReplayLogs,
                new PropertyMetadata(OnShouldRemoveReplayLogsChangedInternal));

        public static readonly DependencyProperty ReplayLogSizeStringProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.ReplayLogSizeString);

        private static readonly DependencyPropertyKey OkCommandPropertyKey =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterReadOnlyDependencyProperty(
                obj => obj.OkCommand);

        private static readonly DependencyPropertyKey CancelCommandPropertyKey =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterReadOnlyDependencyProperty(
                obj => obj.CancelCommand);

        private static readonly PropertyPersistor<RemoveUnwantedFilesWindowViewModel> PropertyPersistor =
            CreateAndInitializePropertyPersistor();

        #endregion

        #region Constructors

        public RemoveUnwantedFilesWindowViewModel()
        {
            OkCommand = new RelayCommand(
                obj => ExecuteLogic(),
                obj => ShouldRemoveRecordingLogs || ShouldRemoveReplayLogs);

            CancelCommand = new RelayCommand(obj => RaiseActionExecuted(false, 0, null));

            OnScriptPathChanged();

            if (!WpfHelper.IsInDesignMode())
            {
                PropertyPersistor.LoadProperties(this);
            }
        }

        #endregion

        #region Events

        public event Action<bool, long, Exception> ActionExecuted;

        #endregion

        #region Public Properties

        public static DependencyProperty OkCommandProperty
        {
            [DebuggerNonUserCode]
            get
            {
                return OkCommandPropertyKey.DependencyProperty;
            }
        }

        public static DependencyProperty CancelCommandProperty
        {
            [DebuggerNonUserCode]
            get
            {
                return CancelCommandPropertyKey.DependencyProperty;
            }
        }

        public string ScriptPath
        {
            get
            {
                return (string)GetValue(ScriptPathProperty);
            }

            set
            {
                SetValue(ScriptPathProperty, value);
            }
        }

        public bool ShouldRemoveRecordingLogs
        {
            get
            {
                return (bool)GetValue(ShouldRemoveRecordingLogsProperty);
            }

            [UsedImplicitly]
            private set
            {
                SetValue(ShouldRemoveRecordingLogsProperty, value);
            }
        }

        public string RecordingLogSizeString
        {
            get
            {
                return (string)GetValue(RecordingLogSizeStringProperty);
            }

            set
            {
                SetValue(RecordingLogSizeStringProperty, value);
            }
        }

        public bool ShouldRemoveReplayLogs
        {
            get
            {
                return (bool)GetValue(ShouldRemoveReplayLogsProperty);
            }

            [UsedImplicitly]
            private set
            {
                SetValue(ShouldRemoveReplayLogsProperty, value);
            }
        }

        public string ReplayLogSizeString
        {
            get
            {
                return (string)GetValue(ReplayLogSizeStringProperty);
            }

            set
            {
                SetValue(ReplayLogSizeStringProperty, value);
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return (ICommand)GetValue(OkCommandProperty);
            }

            private set
            {
                SetValue(OkCommandPropertyKey, value);
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return (ICommand)GetValue(CancelCommandProperty);
            }

            private set
            {
                SetValue(CancelCommandPropertyKey, value);
            }
        }

        #endregion

        #region Private Methods

        private static PropertyPersistor<RemoveUnwantedFilesWindowViewModel> CreateAndInitializePropertyPersistor()
        {
            var propertyPersistor = new PropertyPersistor<RemoveUnwantedFilesWindowViewModel>();
            propertyPersistor.RegisterProperty(obj => obj.ShouldRemoveRecordingLogs);
            propertyPersistor.RegisterProperty(obj => obj.ShouldRemoveReplayLogs);

            return propertyPersistor;
        }

        private static void OnScriptPathChangedInternal(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((RemoveUnwantedFilesWindowViewModel)obj.EnsureNotNull()).OnScriptPathChanged();
        }

        private static void OnShouldRemoveRecordingLogsChangedInternal(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            ((RemoveUnwantedFilesWindowViewModel)obj.EnsureNotNull()).OnShouldRemoveRecordingLogsChanged();
        }

        private static void OnShouldRemoveReplayLogsChangedInternal(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            ((RemoveUnwantedFilesWindowViewModel)obj.EnsureNotNull()).OnShouldRemoveReplayLogsChanged();
        }

        private static FileSystemInfo[] GetFileSystemObjects(
            [NotNull] string scriptDirectory,
            [NotNull] IEnumerable<string> directories,
            [NotNull] IEnumerable<string> files)
        {
            var fileSystemInfoMap = new Dictionary<string, FileSystemInfo>(LocalHelper.FileSystemNameComparer);

            var scriptDirectoryInfo = new DirectoryInfo(scriptDirectory);
            if (!scriptDirectoryInfo.Exists)
            {
                return new FileSystemInfo[0];
            }

            foreach (var file in files)
            {
                var fileInfos = scriptDirectoryInfo.GetFiles(file, SearchOption.TopDirectoryOnly);
                foreach (var fileInfo in fileInfos)
                {
                    if (!fileInfo.Exists || fileSystemInfoMap.ContainsKey(fileInfo.FullName))
                    {
                        continue;
                    }

                    fileSystemInfoMap.Add(fileInfo.FullName, fileInfo);
                }
            }

            foreach (var directory in directories)
            {
                var directoryInfos = scriptDirectoryInfo.GetDirectories(directory, SearchOption.TopDirectoryOnly);
                foreach (var directoryInfo in directoryInfos)
                {
                    if (!directoryInfo.Exists || fileSystemInfoMap.ContainsKey(directoryInfo.FullName))
                    {
                        continue;
                    }

                    fileSystemInfoMap.Add(directoryInfo.FullName, directoryInfo);

                    var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

                    fileInfos
                        .Where(obj => !fileSystemInfoMap.ContainsKey(obj.FullName))
                        .DoForEach(obj => fileSystemInfoMap.Add(obj.FullName, obj));
                }
            }

            return fileSystemInfoMap.Values.ToArray();
        }

        private static FileSystemInfo[] GetRecordingLogItems([NotNull] string scriptDirectory)
        {
            return GetFileSystemObjects(
                scriptDirectory,
                new[] { "data" },
                new[] { "ThumbnailsCache.tmp" });
        }

        private static FileSystemInfo[] GetReplayLogItems([NotNull] string scriptDirectory)
        {
            return GetFileSystemObjects(
                scriptDirectory,
                new[] { "result*" },
                new[]
                {
                    "*.pickle",
                    "*.ci",
                    "pre_cci.c",
                    "options.txt",
                    "mdrv_cmd.txt",
                    "mdrv.log",
                    "logfile.log",
                    "CompilerLogMetadata.xml"
                });
        }

        private static long GetSize([NotNull] ICollection<FileSystemInfo> fileSystemInfos)
        {
            return fileSystemInfos.Count == 0 ? 0 : fileSystemInfos.OfType<FileInfo>().Sum(obj => obj.Length);
        }

        private static void DeleteFile(FileInfo info)
        {
            info.Refresh();
            if (!info.Exists)
            {
                return;
            }

            info.Attributes = FileAttributes.Normal;
            info.Delete();
        }

        private static void DeleteDirectory(DirectoryInfo info)
        {
            info.Refresh();
            if (!info.Exists)
            {
                return;
            }

            info.Delete(true);
        }

        private static void DeleteItems([NotNull] ICollection<FileSystemInfo> fileSystemInfos)
        {
            var fileInfos = fileSystemInfos.OfType<FileInfo>().ToArray();
            fileInfos.DoForEach(DeleteFile);

            var directoryInfos = fileSystemInfos.OfType<DirectoryInfo>().ToArray();
            directoryInfos.DoForEach(DeleteDirectory);
        }

        private void RaiseOkCommandCanExecuteChanged()
        {
            var relayCommand = OkCommand as RelayCommand;
            if (relayCommand != null)
            {
                relayCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnScriptPathChanged()
        {
            if (ScriptPath.IsNullOrWhiteSpace())
            {
                RecordingLogSizeString = ReplayLogSizeString = "?";
                return;
            }

            var scriptDirectory = Path.GetDirectoryName(ScriptPath);
            if (scriptDirectory.IsNullOrWhiteSpace())
            {
                RecordingLogSizeString = ReplayLogSizeString = "?";
                return;
            }

            var recordingLogItems = GetRecordingLogItems(scriptDirectory.EnsureNotNull());
            var recordingLogSize = GetSize(recordingLogItems);
            RecordingLogSizeString = recordingLogSize.FormatFileSize();

            var replayLogItems = GetReplayLogItems(scriptDirectory.EnsureNotNull());
            var replayLogSize = GetSize(replayLogItems);
            ReplayLogSizeString = replayLogSize.FormatFileSize();
        }

        private void OnShouldRemoveRecordingLogsChanged()
        {
            RaiseOkCommandCanExecuteChanged();
        }

        private void OnShouldRemoveReplayLogsChanged()
        {
            RaiseOkCommandCanExecuteChanged();
        }

        private void RaiseActionExecuted(bool logicExecuted, long totalSize, Exception exception)
        {
            var handler = ActionExecuted;
            if (handler != null)
            {
                handler(logicExecuted, totalSize, exception);
            }
        }

        private void ExecuteLogic()
        {
            PropertyPersistor.SaveProperties(this);

            if (ScriptPath.IsNullOrWhiteSpace())
            {
                RaiseActionExecuted(true, 0, null);
                return;
            }

            var scriptDirectory = Path.GetDirectoryName(ScriptPath);
            if (scriptDirectory.IsNullOrWhiteSpace())
            {
                RaiseActionExecuted(true, 0, null);
                return;
            }

            long totalSize;
            try
            {
                var items = new List<FileSystemInfo>();
                if (ShouldRemoveRecordingLogs)
                {
                    var recordingLogItems = GetRecordingLogItems(scriptDirectory.EnsureNotNull());
                    items.AddRange(recordingLogItems);
                }

                if (ShouldRemoveReplayLogs)
                {
                    var replayLogItems = GetReplayLogItems(scriptDirectory.EnsureNotNull());
                    items.AddRange(replayLogItems);
                }

                totalSize = GetSize(items);
                DeleteItems(items);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                Trace.TraceError(
                    "[{0}] Error has occurred: {1}",
                    MethodBase.GetCurrentMethod().GetQualifiedName(),
                    ex);

                RaiseActionExecuted(true, 0, ex);
                return;
            }

            RaiseActionExecuted(true, totalSize, null);
        }

        #endregion
    }
}