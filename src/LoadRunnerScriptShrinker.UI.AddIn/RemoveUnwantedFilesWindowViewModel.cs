using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Data;
using MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.Commands;
using Omnifactotum.Annotations;
using Omnifactotum.Wpf;
using static
    Omnifactotum.Wpf.WpfFactotum.For<MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn.RemoveUnwantedFilesWindowViewModel>;
using ICommand = System.Windows.Input.ICommand;

namespace MyLoadTest.LoadRunnerScriptShrinker.UI.AddIn
{
    internal sealed class RemoveUnwantedFilesWindowViewModel : DependencyObject
    {
        #region Constants and Fields

        public static readonly DependencyProperty ScriptPathProperty =
            RegisterDependencyProperty(
                obj => obj.ScriptPath,
                new PropertyMetadata(null, OnScriptPathChangedInternal));

        public static readonly DependencyProperty ShouldRemoveRecordingLogsProperty =
            RegisterDependencyProperty(
                obj => obj.ShouldRemoveRecordingLogs,
                new PropertyMetadata(OnShouldRemoveRecordingLogsChangedInternal));

        public static readonly DependencyProperty RecordingLogSizeStringProperty =
            RegisterDependencyProperty(
                obj => obj.RecordingLogSizeString);

        public static readonly DependencyProperty ShouldRemoveReplayLogsProperty =
            RegisterDependencyProperty(
                obj => obj.ShouldRemoveReplayLogs,
                new PropertyMetadata(OnShouldRemoveReplayLogsChangedInternal));

        public static readonly DependencyProperty ReplayLogSizeStringProperty =
            RegisterDependencyProperty(
                obj => obj.ReplayLogSizeString);

        private static readonly DependencyPropertyKey OkCommandPropertyKey =
            RegisterReadOnlyDependencyProperty(
                obj => obj.DeleteCommand);

        private static readonly DependencyPropertyKey CancelCommandPropertyKey =
            RegisterReadOnlyDependencyProperty(
                obj => obj.CloseCommand);

        private static readonly PropertyPersistor<RemoveUnwantedFilesWindowViewModel> PropertyPersistor =
            CreateAndInitializePropertyPersistor();

        private readonly ObservableCollection<LogRecord> _logRecordsInternal;

        #endregion

        #region Constructors

        public RemoveUnwantedFilesWindowViewModel()
        {
            _logRecordsInternal = new ObservableCollection<LogRecord>();
            LogRecords = new CollectionView(_logRecordsInternal);

            DeleteCommand = new RelayCommand(
                obj => ExecuteDeleteCommand(),
                obj => ShouldRemoveRecordingLogs || ShouldRemoveReplayLogs);

            CloseCommand = new RelayCommand(ExecuteCloseCommand);

            RefreshInformation();

            if (!WpfFactotum.IsInDesignMode())
            {
                PropertyPersistor.LoadProperties(this);
            }
        }

        #endregion

        #region Events

        public event EventHandler Closed;

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

        public CollectionView LogRecords
        {
            get;
            private set;
        }

        public ICommand DeleteCommand
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

        public ICommand CloseCommand
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

        private static long GetTotalSize([NotNull] ICollection<FileSystemInfo> fileSystemInfos)
        {
            return fileSystemInfos.Count == 0 ? 0 : fileSystemInfos.OfType<FileInfo>().Sum(obj => obj.Length);
        }

        private void DeleteFile(FileInfo info)
        {
            try
            {
                info.Refresh();
                if (!info.Exists)
                {
                    return;
                }

                info.Attributes = FileAttributes.Normal;
                info.Delete();
            }
            catch (Exception ex)
                when (ex is IOException || ex is SecurityException || ex is UnauthorizedAccessException)
            {
                AddLogRecord(LogRecordType.Error, @"Unable to delete file ""{0}"": {1}", info.FullName, ex.Message);
                return;
            }

            AddLogRecord(LogRecordType.Info, @"File ""{0}"" has been deleted.", info.FullName);
        }

        private void DeleteDirectory(DirectoryInfo info)
        {
            try
            {
                info.Refresh();
                if (!info.Exists)
                {
                    return;
                }

                info.Delete(true);
            }
            catch (Exception ex)
                when (ex is IOException || ex is SecurityException || ex is UnauthorizedAccessException)
            {
                AddLogRecord(
                    LogRecordType.Error,
                    @"Unable to delete directory ""{0}"": {1}",
                    info.FullName,
                    ex.Message);

                return;
            }

            AddLogRecord(LogRecordType.Info, @"Directory ""{0}"" has been deleted.", info.FullName);
        }

        private void DeleteItems([NotNull] ICollection<FileSystemInfo> fileSystemInfos)
        {
            var fileInfos = fileSystemInfos.OfType<FileInfo>().ToArray();
            fileInfos.DoForEach(DeleteFile);

            var directoryInfos = fileSystemInfos.OfType<DirectoryInfo>().ToArray();
            directoryInfos.DoForEach(DeleteDirectory);
        }

        private void RaiseDeleteCommandCanExecuteChanged()
        {
            var relayCommand = DeleteCommand as RelayCommand;
            relayCommand?.RaiseCanExecuteChanged();
        }

        private void RefreshInformation()
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
            var recordingLogSize = GetTotalSize(recordingLogItems);
            RecordingLogSizeString = recordingLogSize.FormatFileSize();

            var replayLogItems = GetReplayLogItems(scriptDirectory.EnsureNotNull());
            var replayLogSize = GetTotalSize(replayLogItems);
            ReplayLogSizeString = replayLogSize.FormatFileSize();
        }

        private void OnScriptPathChanged()
        {
            RefreshInformation();
        }

        private void OnShouldRemoveRecordingLogsChanged()
        {
            RaiseDeleteCommandCanExecuteChanged();
        }

        private void OnShouldRemoveReplayLogsChanged()
        {
            RaiseDeleteCommandCanExecuteChanged();
        }

        private void ClearLog()
        {
            _logRecordsInternal.Clear();
        }

        private void AddLogRecord(LogRecordType type, string message)
        {
            _logRecordsInternal.Add(new LogRecord(type, message));
        }

        [StringFormatMethod("format")]
        private void AddLogRecord(LogRecordType type, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            AddLogRecord(type, message);
        }

        private void ExecuteDeleteCommand()
        {
            ExecuteDeleteCommandInternal();
            RefreshInformation();
        }

        private void ExecuteDeleteCommandInternal()
        {
            PropertyPersistor.SaveProperties(this);

            ClearLog();

            if (ScriptPath.IsNullOrWhiteSpace())
            {
                AddLogRecord(LogRecordType.Error, "The script path is empty.");
                return;
            }

            var scriptDirectory = Path.GetDirectoryName(ScriptPath);
            if (scriptDirectory.IsNullOrWhiteSpace())
            {
                AddLogRecord(LogRecordType.Error, "Unable to determine the script path directory.");
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

                totalSize = GetTotalSize(items);
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

                AddLogRecord(LogRecordType.Error, "Error has occurred: {0}", ex.Message);
                return;
            }

            AddLogRecord(
                LogRecordType.Info,
                "The selected files have been removed (size: {0}).",
                totalSize.FormatFileSize());
        }

        private void ExecuteCloseCommand(object obj)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}