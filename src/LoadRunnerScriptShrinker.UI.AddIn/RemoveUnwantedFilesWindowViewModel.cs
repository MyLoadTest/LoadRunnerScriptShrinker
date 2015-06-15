using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Omnifactotum.Annotations;

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
                obj => obj.ShouldRemoveRecordingLogs);

        public static readonly DependencyProperty RecordingLogSizeStringProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.RecordingLogSizeString);

        public static readonly DependencyProperty ShouldRemoveReplayLogsProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.ShouldRemoveReplayLogs);

        public static readonly DependencyProperty ReplayLogSizeStringProperty =
            WpfHelper.For<RemoveUnwantedFilesWindowViewModel>.RegisterDependencyProperty(
                obj => obj.ReplayLogSizeString);

        #endregion

        #region Constructors

        public RemoveUnwantedFilesWindowViewModel()
        {
            OnScriptPathChanged();
        }

        #endregion

        #region Public Properties

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

            set
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

            set
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

        #endregion

        #region Private Methods

        private static void OnScriptPathChangedInternal(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((RemoveUnwantedFilesWindowViewModel)obj.EnsureNotNull()).OnScriptPathChanged();
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

        private static FileSystemInfo[] GetRecordLogItems([NotNull] string scriptDirectory)
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

            var recordLogItems = GetRecordLogItems(scriptDirectory.EnsureNotNull());
            var recordLogSize = GetSize(recordLogItems);
            RecordingLogSizeString = recordLogSize.FormatFileSize();

            var replayLogItems = GetReplayLogItems(scriptDirectory.EnsureNotNull());
            var replayLogSize = GetSize(replayLogItems);
            ReplayLogSizeString = replayLogSize.FormatFileSize();
        }

        #endregion
    }
}