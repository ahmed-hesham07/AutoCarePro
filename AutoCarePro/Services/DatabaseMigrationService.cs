using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoCarePro.Data;
using System.Threading.Tasks;

namespace AutoCarePro.Services
{
    /// <summary>
    /// DatabaseMigrationService class handles database migrations, backups, and validation.
    /// It provides functionality to safely update the database schema while maintaining
    /// data integrity through backup and restore capabilities.
    /// </summary>
    public class DatabaseMigrationService : IDisposable
    {
        // Directory where database backups are stored
        private readonly string _backupDirectory = "DatabaseBackups";
        // Database context for performing migrations
        private readonly AutoCareProContext _context;
        private bool _disposed = false;

        /// <summary>
        /// Constructor initializes the service and ensures the backup directory exists
        /// </summary>
        public DatabaseMigrationService()
        {
            var options = new DbContextOptionsBuilder<AutoCareProContext>()
                .UseSqlite(GetConnectionString())
                .Options;
            _context = new AutoCareProContext(options);
            EnsureBackupDirectoryExists();
        }

        private string GetConnectionString()
        {
            return "Data Source=AutoCarePro.db";
        }

        /// <summary>
        /// Ensures that the backup directory exists, creating it if necessary
        /// </summary>
        private void EnsureBackupDirectoryExists()
        {
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        /// <summary>
        /// Applies any pending database migrations safely with backup and validation
        /// </summary>
        /// <exception cref="Exception">Thrown if migration fails</exception>
        public async Task ApplyMigrationsAsync()
        {
            try
            {
                await CreateBackupAsync("pre_migration");
                
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Database.MigrateAsync();
                    await ValidateDatabaseAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await RestoreBackupAsync("pre_migration");
                throw new Exception($"Database migration failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a backup of the database with the specified name
        /// </summary>
        /// <param name="backupName">Name to identify the backup</param>
        /// <exception cref="Exception">Thrown if backup creation fails</exception>
        public async Task CreateBackupAsync(string backupName)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(_backupDirectory, $"{backupName}_{timestamp}.db");
                
                // Close the connection before copying the file
                await _context.Database.CloseConnectionAsync();
                
                // Copy the database file
                File.Copy("AutoCarePro.db", backupPath, true);
                
                // Reopen the connection
                await _context.Database.OpenConnectionAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create database backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restores a database backup with the specified name
        /// </summary>
        /// <param name="backupName">Name of the backup to restore</param>
        /// <exception cref="Exception">Thrown if backup restoration fails</exception>
        public async Task RestoreBackupAsync(string backupName)
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDirectory, $"{backupName}_*.db")
                    .OrderByDescending(f => f)
                    .FirstOrDefault();

                if (backupFiles == null)
                {
                    throw new Exception($"No backup found with name: {backupName}");
                }

                // Close the connection before restoring
                await _context.Database.CloseConnectionAsync();
                
                // Copy the backup file to the database location
                File.Copy(backupFiles, "AutoCarePro.db", true);
                
                // Reopen the connection
                await _context.Database.OpenConnectionAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restore database backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates the database structure by checking for required tables and indexes
        /// </summary>
        /// <exception cref="Exception">Thrown if validation fails</exception>
        private async Task ValidateDatabaseAsync()
        {
            try
            {
                var tables = new[] { "Users", "Vehicles", "MaintenanceRecords", "MaintenanceRecommendations", "DiagnosisRecommendations" };
                foreach (var table in tables)
                {
                    if (!await TableExistsAsync(table))
                    {
                        throw new Exception($"Required table '{table}' is missing");
                    }
                }

                var indexes = new[] { "IX_Users_Username", "IX_Users_Email", "IX_Vehicles_UserId" };
                foreach (var index in indexes)
                {
                    if (!await IndexExistsAsync(index))
                    {
                        throw new Exception($"Required index '{index}' is missing");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database validation failed: {ex.Message}", ex);
            }
        }

        private async Task<bool> TableExistsAsync(string table)
        {
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM sqlite_master WHERE type='table' AND name='{table}'");
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IndexExistsAsync(string index)
        {
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM sqlite_master WHERE type='index' AND name='{index}'");
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cleans up old database backups that are older than the specified number of days
        /// </summary>
        /// <param name="daysToKeep">Number of days to keep backups (default: 7)</param>
        /// <exception cref="Exception">Thrown if cleanup fails</exception>
        public async Task CleanupOldBackupsAsync(int daysToKeep = 7)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var backupFiles = Directory.GetFiles(_backupDirectory, "*.db");

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        await Task.Run(() => fileInfo.Delete());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cleanup old backups: {ex.Message}", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DatabaseMigrationService()
        {
            Dispose(false);
        }
    }
} 