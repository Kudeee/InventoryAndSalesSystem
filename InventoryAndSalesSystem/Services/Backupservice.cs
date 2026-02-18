using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;

namespace InventoryAndSalesSystem.Services
{
    /// <summary>
    /// Handles automatic weekly backups of all Excel data files.
    /// Backs up to Data/Backups/ folder, keeps last 8 backups (2 months).
    /// </summary>
    public class BackupService : IDisposable
    {
        private readonly string _dataFolder;
        private readonly string _backupFolder;
        private readonly string _lastBackupFile;
        private Timer? _timer;
        private bool _disposed;

        private static readonly TimeSpan BackupInterval = TimeSpan.FromDays(7);
        private const int MaxBackupsToKeep = 8;

        public BackupService(string dataFolder)
        {
            _dataFolder = dataFolder;
            _backupFolder = Path.Combine(dataFolder, "Backups");
            _lastBackupFile = Path.Combine(_backupFolder, ".last_backup");

            Directory.CreateDirectory(_backupFolder);
        }

        /// <summary>
        /// Starts the weekly backup timer. Runs backup immediately if overdue.
        /// </summary>
        public void Start()
        {
            if (IsBackupOverdue())
                RunBackup();

            // Check every hour if backup is due
            _timer = new Timer(_ => CheckAndRunBackup(), null,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(1));
        }

        private void CheckAndRunBackup()
        {
            if (IsBackupOverdue())
                RunBackup();
        }

        private bool IsBackupOverdue()
        {
            if (!File.Exists(_lastBackupFile))
                return true;

            var lastBackupText = File.ReadAllText(_lastBackupFile).Trim();
            if (!DateTime.TryParse(lastBackupText, out var lastBackup))
                return true;

            return DateTime.Now - lastBackup >= BackupInterval;
        }

        /// <summary>
        /// Performs the backup immediately, regardless of schedule.
        /// Returns the path of the created backup file.
        /// </summary>
        public string RunBackup()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFileName = $"Backup_{timestamp}.zip";
            var backupPath = Path.Combine(_backupFolder, backupFileName);

            using (var zip = ZipFile.Open(backupPath, ZipArchiveMode.Create))
            {
                foreach (var xlsxFile in Directory.GetFiles(_dataFolder, "*.xlsx"))
                {
                    zip.CreateEntryFromFile(xlsxFile, Path.GetFileName(xlsxFile));
                }

                // Include a manifest with backup metadata
                var manifestEntry = zip.CreateEntry("backup_manifest.txt");
                using var writer = new StreamWriter(manifestEntry.Open());
                writer.WriteLine($"Backup Date: {DateTime.Now:MMMM dd, yyyy hh:mm:ss tt}");
                writer.WriteLine($"Backup File: {backupFileName}");
                writer.WriteLine($"Files Included:");
                foreach (var xlsxFile in Directory.GetFiles(_dataFolder, "*.xlsx"))
                    writer.WriteLine($"  - {Path.GetFileName(xlsxFile)}");
            }

            // Record the last backup time
            File.WriteAllText(_lastBackupFile, DateTime.Now.ToString("o"));

            // Prune old backups
            PruneOldBackups();

            return backupPath;
        }

        private void PruneOldBackups()
        {
            var backups = Directory.GetFiles(_backupFolder, "Backup_*.zip");
            Array.Sort(backups); // Oldest first (lexicographic = chronological with timestamp names)

            while (backups.Length > MaxBackupsToKeep)
            {
                File.Delete(backups[0]);
                backups = Directory.GetFiles(_backupFolder, "Backup_*.zip");
                Array.Sort(backups);
            }
        }

        /// <summary>
        /// Returns info about the last backup (date and file path), or null if none.
        /// </summary>
        public (DateTime? LastBackupDate, string[] BackupFiles) GetBackupInfo()
        {
            DateTime? lastDate = null;
            if (File.Exists(_lastBackupFile))
            {
                var text = File.ReadAllText(_lastBackupFile).Trim();
                if (DateTime.TryParse(text, out var dt))
                    lastDate = dt;
            }

            var files = Directory.GetFiles(_backupFolder, "Backup_*.zip");
            Array.Sort(files);
            Array.Reverse(files); // Newest first
            return (lastDate, files);
        }

        public string BackupFolder => _backupFolder;

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Dispose();
                _disposed = true;
            }
        }
    }
}