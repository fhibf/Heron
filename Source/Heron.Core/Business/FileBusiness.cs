using Heron.Core.Data;
using Heron.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Heron.Core.Business
{
    public class FileBusiness {

        private BackupEnvironment _currentEnvironment;

        public event FileCopyHandler OnFileBackuped;

        public FileBusiness(BackupEnvironment environment) {

            this._currentEnvironment = environment;
        }

        public string GetBackupDestiny(string path) {

            string destiny = string.Empty;

            string currentDrive = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("\\") + 1);

            destiny = Path.Combine(currentDrive,                                   
                "bkp",
                this._currentEnvironment.Name,
                path.Replace(":", "$"));

            return destiny;
        }

        public ValidationResult ValidateFolders() {

            var repository = new FilesRepository();

            foreach (var iFolder in this._currentEnvironment.Folders) {

                if (!repository.DirectoryExists(iFolder.Path))
                    return new ValidationResult(string.Format("The path \"{0}\" is invalid. It doesn't exist.", iFolder.Path));
            }

            return null;
        }

        public IEnumerable<ComparisonResult> CompareFiles() {

            List<ComparisonResult> returnValue = new List<ComparisonResult>();

            foreach (var iFolder in this._currentEnvironment.Folders) {

                var resultComparison = CompareFiles(iFolder.Path);

                if (resultComparison != null && resultComparison.Count() > 0)
                    returnValue.AddRange(resultComparison);
            }

            return returnValue;
        }

        public int GetTotalFiles() {

            int totalFiles = 0;

            foreach (var iFolder in this._currentEnvironment.Folders) {

                var repository = new FilesRepository();

                totalFiles += repository.GetTotalFiles(iFolder.Path);
            }

            return totalFiles;
        }

        public void Backup() {

            BackupFolders();

            UpdateBackupDate();
        }

        public void UpdateBackupDate(){

            this._currentEnvironment.LastBackup = DateTime.Now;

            EnvironmentRepository repository = new EnvironmentRepository();
            repository.SaveBackupDate(this._currentEnvironment);
        }

        private void BackupFolders() {

            foreach (var iFolder in this._currentEnvironment.Folders) {

                Backup(iFolder.Path);
            }
        }

        private void Backup(string path) {

            var repository = new FilesRepository();

            repository.OnBackupFile += OnBackupFile;

            var backupDestiny = GetBackupDestiny(path);
            
            repository.CopyDirectoryTo(path, backupDestiny);
        }

        private void OnBackupFile(object sender, FileCopyEventArgs e) {

            if (OnFileBackuped != null)
                this.OnFileBackuped(this, e);
        }
        
        private IEnumerable<ComparisonResult> CompareFiles(string path) {

            var repository = new FilesRepository();

            var backupDestiny = GetBackupDestiny(path);
            repository.CreateDirectoryIfNonExists(backupDestiny);

            //var originFiles = repository.GetAllFiles(path);
            //var destinyFiles = repository.GetAllFiles(backupDestiny);
            
            return null;
        }
    }
}
