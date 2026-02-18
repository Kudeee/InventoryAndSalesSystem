using InventoryAndSalesSystem.Helpers;
using InventoryAndSalesSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace InventoryAndSalesSystem.ViewModels
{
    public class BackupViewModel : INotifyPropertyChanged
    {
        private readonly BackupService _backupService;
        private readonly AuditTrailService _auditService;
        private ObservableCollection<BackupFileInfo> _backupFiles = new();
        private string _statusMessage = "";
        private DateTime? _lastBackupDate;

        public BackupViewModel(BackupService backupService, AuditTrailService auditService)
        {
            _backupService = backupService;
            _auditService = auditService;

            BackupNowCommand = new RelayCommand(_ => RunManualBackup());
            OpenFolderCommand = new RelayCommand(_ => OpenBackupFolder());
            RefreshCommand = new RelayCommand(_ => LoadBackupInfo());

            LoadBackupInfo();
        }

        public ObservableCollection<BackupFileInfo> BackupFiles
        {
            get => _backupFiles;
            set { _backupFiles = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public DateTime? LastBackupDate
        {
            get => _lastBackupDate;
            set { _lastBackupDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastBackupDisplay)); }
        }

        public string LastBackupDisplay => LastBackupDate.HasValue
            ? LastBackupDate.Value.ToString("MMMM dd, yyyy hh:mm tt")
            : "Never";

        public string NextBackupDisplay => LastBackupDate.HasValue
            ? LastBackupDate.Value.AddDays(7).ToString("MMMM dd, yyyy")
            : "As soon as possible";

        public int TotalBackups => BackupFiles.Count;

        public ICommand BackupNowCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand RefreshCommand { get; }

        private void LoadBackupInfo()
        {
            var (lastDate, files) = _backupService.GetBackupInfo();
            LastBackupDate = lastDate;

            BackupFiles.Clear();
            foreach (var f in files)
            {
                var info = new FileInfo(f);
                BackupFiles.Add(new BackupFileInfo
                {
                    FileName = info.Name,
                    FilePath = f,
                    CreatedDate = info.CreationTime,
                    SizeKb = info.Length / 1024.0
                });
            }

            OnPropertyChanged(nameof(TotalBackups));
            OnPropertyChanged(nameof(NextBackupDisplay));
        }

        private void RunManualBackup()
        {
            try
            {
                StatusMessage = "Creating backup…";
                var path = _backupService.RunBackup();
                _auditService.LogManualBackup();
                LoadBackupInfo();
                StatusMessage = $"✅ Backup created: {Path.GetFileName(path)}";
                MessageBox.Show($"Backup created successfully!\n\n{path}",
                    "Backup Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Backup failed: {ex.Message}";
                MessageBox.Show($"Backup failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBackupFolder()
        {
            if (Directory.Exists(_backupService.BackupFolder))
                Process.Start("explorer.exe", _backupService.BackupFolder);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class BackupFileInfo
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public double SizeKb { get; set; }
        public string CreatedDisplay => CreatedDate.ToString("MMM dd, yyyy hh:mm tt");
        public string SizeDisplay => $"{SizeKb:F1} KB";
    }
}