using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCP.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DCP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _cleanupPath = string.Empty;

        [ObservableProperty]
        private string _destPath = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyFilesCommand))]
        private bool _canCopyFiles = false;

        [ObservableProperty]
        private ObservableCollection<DashCamVideo> _files = [];

        [ObservableProperty]
        private ObservableCollection<DashCamVideo> _filesCopied = [];

        [ObservableProperty]
        private bool _canEmptyItem = false;
        [ObservableProperty]
        private bool _canClearAll = true;

        private UserData _userData;


        public MainWindowViewModel()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (File.Exists(Path.Combine(appDataPath, "user.json")))
            {
                string json = File.ReadAllText(Path.Combine(appDataPath, "user.json"));
                _userData = JsonSerializer.Deserialize<UserData>(json);
                CleanupPath = _userData.SourcePath;
                DestPath = _userData.DestPath;
            }
        }
        partial void OnCleanupPathChanged(string value)
        {
            CanCopyFiles = AssertCanCopyFiles();
        }
        partial void OnDestPathChanged(string value)
        {
            CanCopyFiles = AssertCanCopyFiles();
        }

        private bool AssertCanCopyFiles()
        {
            return Path.Exists(CleanupPath) && Path.Exists(DestPath);
        }

        [RelayCommand(CanExecute = nameof(CanCopyFiles))]
        private async Task CopyFiles()
        {
            _userData = new UserData { SourcePath = CleanupPath, DestPath = DestPath };
            string json = JsonSerializer.Serialize(_userData);
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(folder);
            await Task.Run(() => File.WriteAllTextAsync(Path.Combine(folder, "user.json"), json));

            var paths = new ObservableCollection<string>(Directory.GetFiles(CleanupPath, "*.mp4"));
            foreach (var path in paths)
            {
                var front = path.Substring(path.LastIndexOf('_') + 1, 1) == "F";
                var item = new DashCamVideo(path, front);
                Files.Add(item);
                FilesCopied.Add(item);
            }
            try
            {
                foreach (var file in Files)
                {
                    if (!Directory.Exists(Path.Combine(DestPath, file.Date)))
                    {
                        Directory.CreateDirectory(Path.Combine(DestPath, file.Date));
                    }
                    if (!Directory.Exists(Path.Combine(DestPath, file.Date, "AV")))
                    {
                        Directory.CreateDirectory(Path.Combine(DestPath, file.Date, "AV"));
                    }
                    if (!Directory.Exists(Path.Combine(DestPath, file.Date, "AR")))
                    {
                        Directory.CreateDirectory(Path.Combine(DestPath, file.Date, "AR"));
                    }
                    string position;
                    if (file.IsFront)
                    {
                        position = "AV";
                    }
                    else
                    {
                        position = "AR";
                    }
                    await Task.Run(() => CopyFileAsync(file.VideoPath, Path.Combine(DestPath, file.Date, position, file.Name + file.Extension), p =>
                    {
                        file.Progress = p;
                        if (file.Progress == 100)
                        {
                            CanEmptyItem = true;
                        }
                    }));
                }
            }
            finally
            {
                Files.Clear();
            }
            
            
        }

        private async Task CopyFileAsync(string inPath,
            string outPath,
            Action<int> onProgress = null,
            CancellationToken cancellationtoken = default)
        {
            const int bufferSize = 1*1024*1024; //10MB
            await using var source = new FileStream(
                inPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize,
                true
                );
            await using var dest = new FileStream(
                outPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                true
                );
            var buffer = new byte[bufferSize];

            long totalBytes = source.Length;
            long copiedBytes = 0;

            int bytesRead;
            int lastPercent = -1;

            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationtoken)) > 0)
            {

                await dest.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationtoken);
                copiedBytes += bytesRead;
                int percent = totalBytes == 0
                    ? 100
                    : (int)((double)copiedBytes / totalBytes * 100);
                if ( percent != lastPercent)
                {
                    onProgress?.Invoke(percent);
                }
                
            }
        }

        [RelayCommand]
        private async Task BrowseCleanupFiles()
        {
            var dialog = await App.TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Choisissez le fichier à scanner",
                AllowMultiple = false
            });

            if (dialog.Count > 0)
            {
                CleanupPath = dialog[0].TryGetLocalPath() ?? String.Empty;
            }
        }
        [RelayCommand]
        private async Task BrowseDestinationFiles()
        {
            var dialog = await App.TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Choisissez l'emplacement du fichier où déposer les vidéos",
                AllowMultiple = false
            });

            if (dialog.Count > 0)
            {
                DestPath = dialog[0].TryGetLocalPath() ?? String.Empty;
            }
        }
        [RelayCommand]
        public void RemoveVideo(DashCamVideo item)
        {
            FilesCopied.Remove(item);
        }

        [RelayCommand]
        public void ClearAllVideos()
        {
            List<int> toRemove = new List<int>();
            try
            {
                foreach (var item in FilesCopied)
                {

                    if (item.Progress == 100)
                    {
                        toRemove.Add(FilesCopied.IndexOf(item));
                       //FilesCopied.Remove(item);
                    }
                }
            }
            finally
            {
                for(int i = toRemove.Count-1; i >= 0; i--)
                {
                    FilesCopied.RemoveAt(toRemove[i]);
                }
            }
            
        }
    }
}