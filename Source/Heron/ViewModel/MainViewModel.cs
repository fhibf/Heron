using Heron.Common;
using Heron.Core.Business;
using Heron.Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Heron.ViewModel {
    internal class MainViewModel : Common.NotifyPropertyChangedBase {

        #region [ Private attributes ]

        private BackupEnvironment _environment;
        private IIOService _ioService;
        private IMessengerService _iMessenger;
        private BackgroundWorker _worker;
        
        #endregion

        #region [ Properties ]

        public double Progress {
            get {
                return GetValue(() => this.Progress);
            }
            set {
                SetValue(() => this.Progress, value);
            }
        }

        public double MaxProgress {
            get {
                return GetValue(() => this.MaxProgress);
            }
            set {
                SetValue(() => this.MaxProgress, value);
            }
        }

        public string CurrentFileName {
            get {
                return GetValue(() => this.CurrentFileName);
            }
            set {
                SetValue(() => this.CurrentFileName, value);
            }
        }

        public bool IsControlEnabled {
            get { return GetValue(() => this.IsControlEnabled); }
            set { SetValue(() => this.IsControlEnabled, value); }
        }

        public bool IsBackupEnabled {
            get { return GetValue(() => this.IsBackupEnabled); }
            set { SetValue(() => this.IsBackupEnabled, value); }
        }

        public string MachineName {
            get { return GetValue(() => this.MachineName); }
            set { SetValue(() => this.MachineName, value); } 
        }

        public string LastBackup {
            get { return GetValue(() => this.LastBackup); }
            set { SetValue(() => this.LastBackup, value); }
        }

        public string NewFolder {
            get { return GetValue(() => this.NewFolder); }
            set { SetValue(() => this.NewFolder, value); }
        }

        public AsyncObservableCollection<Folder> Folders {
            get { return GetValue(() => this.Folders); }
            set { SetValue(() => this.Folders, value); }
        }

        public string SelectedFolder {
            get { return GetValue(() => this.SelectedFolder); }
            set { SetValue(() => this.SelectedFolder, value); }
        }

        #endregion

        #region [ Events ]

        public ICommand OnBrowseFolders { get; set; }

        public ICommand OnAddFolder { get; set; }

        public ICommand OnSyncFiles { get; set; }

        public ICommand OnBackup { get; set; }

        public Common.DelegateCommand<Folder> OnDeleteFolder { get; set; }

        #endregion

        public MainViewModel() {
            
            this.OnBrowseFolders = new Common.DelegateCommand(BrowseFiles);
            this.OnAddFolder = new Common.DelegateCommand(AddFolder);
            this.OnSyncFiles = new Common.DelegateCommand(SyncFiles);
            this.OnBackup = new Common.DelegateCommand(Backup);
            this.OnDeleteFolder = new Common.DelegateCommand<Folder>(DeleteFolder);

            this.IsControlEnabled = true;

            this._ioService = new Common.WindowsIoService();
            this._iMessenger = new Common.WindowsMessenger();

            this.Folders = new AsyncObservableCollection<Folder>();

            Initialize();

            DisableBackup();

            _worker = new BackgroundWorker();
            _worker.DoWork += DoBackgroundWork;
        }
        
        public void Initialize() {

            var environment = GetCurrentEnvironment();

            if (environment == null)
                return;

            SetEnvironmentDetails(environment);

            UpdateEnvironmentDetails();
        }

        private void UpdateEnvironmentDetails() {
                        
            this.MachineName = this._environment.Name;

            foreach (var iFolder in this._environment.Folders) {

                AddNewFolder(iFolder.Path);
            }

            if (this._environment.LastBackup.HasValue && this._environment.LastBackup.Value > DateTime.MinValue) {

                this.LastBackup = this._environment.LastBackup.Value.ToShortDateString() + " " + 
                                  this._environment.LastBackup.Value.ToShortTimeString();
            }
            else{

                this.LastBackup = "Without backups until now.";
            }
        }

        public void DeleteFolder(Folder folder) {
            
            if (!this.Folders.Contains(folder))
                return;

            this.Folders.Remove(folder);

            DisableBackup();
        }

        public void SetEnvironmentDetails(BackupEnvironment environment) {

            this._environment = environment;
        }

        public BackupEnvironment GetCurrentEnvironment() {

            var business = new EnvironmentBusiness();

            return business.GetCurrentBackupEnvironment();
        }

        private void BrowseFiles() {

            string folder = this._ioService.GetFolderPath();

            if (string.IsNullOrWhiteSpace(folder))
                return;

            this.NewFolder = folder;
        }

        private void AddFolder() {

            if (string.IsNullOrWhiteSpace(this.NewFolder))
                return;

            if (!this._ioService.FolderExists(this.NewFolder))
                return;

            AddNewFolder(this.NewFolder);

            this.NewFolder = string.Empty;

            DisableBackup();
        }

        private void AddNewFolder(string folder) {

            folder = folder.ToLower();

            if (!this.Folders.Any(f => string.Compare(f.Path, folder, StringComparison.CurrentCultureIgnoreCase) == 0))
                this.Folders.Add(new Folder(Guid.NewGuid(), folder));
        }

        private void SyncFiles() {

            UpdateFoldersOfBackupEnvironmentReference();

            SaveBackupEnvironment();

            CompareFiles();

            EnableBackup();
        }

        private void Backup() {

            StartBackupProgress();

            StartBackup();
        }

        private void StartBackupProgress() {

            FileBusiness business = new FileBusiness(this._environment);

            this.MaxProgress = business.GetTotalFiles();
        }

        private void StartBackup() {
            
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();
        }

        private void DoBackgroundWork(object sender, DoWorkEventArgs e) {

            this.IsControlEnabled = false;

            FileBusiness business = new FileBusiness(this._environment);

            business.OnFileBackuped += OnFileBackuped;

            business.Backup();

            UpdateEnvironmentDetails();

            _iMessenger.ShowInformation("Backup completed.");

            HideFileInProgress();
        }

        private void HideFileInProgress() {

            this.Progress = 0;
            this.CurrentFileName = string.Empty;
            this.IsControlEnabled = true;
            this.IsBackupEnabled = false;
        }

        private void OnFileBackuped(object sender, FileCopyEventArgs e) {

            this.Progress += (1.0 / this.MaxProgress);
            this.CurrentFileName = e.FileName; // FormatFileName(e);
        }

        private string FormatFileName(FileCopyEventArgs e) {

            if (string.IsNullOrEmpty(e.FileName))
                return string.Empty;
            if (e.FileName.Length <= 60)
                return e.FileName;

            var returnValue = new StringBuilder();

            string firstPart = e.FileName.Substring(0, 25);
            firstPart = firstPart.Substring(0, firstPart.LastIndexOf("\\"));

            string secondPart = e.FileName.Substring(e.FileName.LastIndexOf("\\"));

            returnValue.Append(firstPart);
            returnValue.Append("(...)");
            returnValue.Append(secondPart);

            return returnValue.ToString();
        }

        private void DisableBackup(){

            this.IsBackupEnabled = false;
        }

        private void EnableBackup() {

            this.IsBackupEnabled = true;
        }

        private bool AreFoldersValid() {

            FileBusiness business = new FileBusiness(this._environment);

            var validationResult = business.ValidateFolders();

            if (validationResult == null) {
                return true;
            }

            _iMessenger.ShowError(validationResult.ErrorMessage);
            return false;
        }

        private void CompareFiles() {

            FileBusiness business = new FileBusiness(this._environment);

            IEnumerable<ComparisonResult> result = business.CompareFiles();
        }

        private void SaveBackupEnvironment() {

            EnvironmentBusiness business = new EnvironmentBusiness();

            business.SaveEnvironmentFolders(this._environment);
        }

        private void UpdateFoldersOfBackupEnvironmentReference(){

            if (this._environment.Folders == null)
                this._environment.Folders = new Collection<Folder>();
            else
                this._environment.Folders.Clear();
            
            foreach (var iFolder in this.Folders) {

                this._environment.Folders.Add(iFolder);
            }
        }
    }
}
