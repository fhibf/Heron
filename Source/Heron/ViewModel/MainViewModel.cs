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

namespace Heron.ViewModel
{
    internal class MainViewModel : NotifyPropertyChangedBase
    {

        #region [ Private attributes ]

        private BackupEnvironment _environment;
        private IIOService _ioService;
        private IMessengerService _iMessenger;
        private BackgroundWorker _worker;

        #endregion

        #region [ Properties ]

        public double Progress
        {
            get
            {
                return GetValue(() => Progress);
            }
            set
            {
                SetValue(() => Progress, value);
            }
        }

        public double MaxProgress
        {
            get
            {
                return GetValue(() => MaxProgress);
            }
            set
            {
                SetValue(() => MaxProgress, value);
            }
        }

        public string CurrentFileName
        {
            get
            {
                return GetValue(() => CurrentFileName);
            }
            set
            {
                SetValue(() => CurrentFileName, value);
            }
        }

        public string OutputMessage
        {
            get
            {
                return GetValue(() => OutputMessage);
            }
            set
            {
                SetValue(() => OutputMessage, value);
            }
        }

        public bool IsControlEnabled
        {
            get { return GetValue(() => IsControlEnabled); }
            set { SetValue(() => IsControlEnabled, value); }
        }

        public bool IsBackupEnabled
        {
            get { return GetValue(() => IsBackupEnabled); }
            set { SetValue(() => IsBackupEnabled, value); }
        }

        public string MachineName
        {
            get { return GetValue(() => MachineName); }
            set { SetValue(() => MachineName, value); }
        }

        public string LastBackup
        {
            get { return GetValue(() => LastBackup); }
            set { SetValue(() => LastBackup, value); }
        }

        public string NewFolder
        {
            get { return GetValue(() => NewFolder); }
            set { SetValue(() => NewFolder, value); }
        }

        public AsyncObservableCollection<Folder> Folders
        {
            get { return GetValue(() => Folders); }
            set { SetValue(() => Folders, value); }
        }

        public string SelectedFolder
        {
            get { return GetValue(() => SelectedFolder); }
            set { SetValue(() => SelectedFolder, value); }
        }

        #endregion

        #region [ Events ]

        public ICommand OnBrowseFolders { get; set; }

        public ICommand OnAddFolder { get; set; }

        public ICommand OnSyncFiles { get; set; }

        public ICommand OnBackup { get; set; }

        public DelegateCommand<Folder> OnDeleteFolder { get; set; }

        #endregion

        public MainViewModel()
        {
            OnBrowseFolders = new DelegateCommand(BrowseFiles);
            OnAddFolder = new DelegateCommand(AddFolder);
            OnSyncFiles = new DelegateCommand(SyncFiles);
            OnBackup = new DelegateCommand(Backup);
            OnDeleteFolder = new DelegateCommand<Folder>(DeleteFolder);

            IsControlEnabled = true;

            _ioService = new WindowsIoService();
            _iMessenger = new WindowsMessenger();

            Folders = new AsyncObservableCollection<Folder>();

            Initialize();

            DisableBackup();

            _worker = new BackgroundWorker();
            _worker.DoWork += DoBackgroundWork;
        }

        public void Initialize()
        {
            var environment = GetCurrentEnvironment();

            if (environment == null)
                return;

            SetEnvironmentDetails(environment);

            UpdateEnvironmentDetails();
        }

        private void UpdateEnvironmentDetails()
        {
            MachineName = _environment.Name;

            foreach (var iFolder in _environment.Folders)
            {

                AddNewFolder(iFolder.Path);
            }

            if (_environment.LastBackup.HasValue && _environment.LastBackup.Value > DateTime.MinValue)
            {

                LastBackup = _environment.LastBackup.Value.ToShortDateString() + " " +
                                  _environment.LastBackup.Value.ToShortTimeString();
            }
            else
            {

                LastBackup = "Without backups until now.";
            }
        }

        public void DeleteFolder(Folder folder)
        {
            if (!Folders.Contains(folder))
                return;

            Folders.Remove(folder);

            DisableBackup();
        }

        public void SetEnvironmentDetails(BackupEnvironment environment)
        {
            _environment = environment;
        }

        public BackupEnvironment GetCurrentEnvironment()
        {
            var business = new EnvironmentBusiness();

            return business.GetCurrentBackupEnvironment();
        }

        private void BrowseFiles()
        {
            string folder = _ioService.GetFolderPath();

            if (string.IsNullOrWhiteSpace(folder))
                return;

            NewFolder = folder;
        }

        private void AddFolder()
        {
            if (string.IsNullOrWhiteSpace(NewFolder))
                return;

            if (!_ioService.FolderExists(NewFolder))
                return;

            AddNewFolder(NewFolder);

            NewFolder = string.Empty;

            DisableBackup();
        }

        private void AddNewFolder(string folder)
        {
            folder = folder.ToLower();

            if (!Folders.Any(f => string.Compare(f.Path, folder, StringComparison.CurrentCultureIgnoreCase) == 0))
                Folders.Add(new Folder(Guid.NewGuid(), folder));
        }

        private void SyncFiles()
        {
            UpdateFoldersOfBackupEnvironmentReference();

            SaveBackupEnvironment();

            CompareFiles();

            EnableBackup();
        }

        private void Backup()
        {
            try
            {
                Progress = 0;
                OutputMessage = string.Empty;

                IsBackupEnabled = false;

                StartBackupProgress();

                StartBackup();
            }
            finally
            {
                IsBackupEnabled = true;
            }
        }

        private void StartBackupProgress()
        {
            FileBusiness business = new FileBusiness(_environment);

            MaxProgress = business.GetTotalFiles();
        }

        private void StartBackup()
        {
            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
        }

        private void DoBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                IsControlEnabled = false;

                FileBusiness business = new FileBusiness(_environment);

                business.OnFileBackuped += OnFileBackuped;

                business.Backup();

                UpdateEnvironmentDetails();

                _iMessenger.ShowInformation("Backup completed.");

                HideFileInProgress();
            }
            catch (Exception ex)
            {
                OutputMessage = ex.Message;
            }
            finally
            {
                IsControlEnabled = true;
            }
        }

        private void HideFileInProgress()
        {
            Progress = 0;
            CurrentFileName = string.Empty;
            IsControlEnabled = true;
            IsBackupEnabled = false;
        }

        private void OnFileBackuped(object sender, FileCopyEventArgs e)
        {
            Progress += (1.0 / MaxProgress);
            CurrentFileName = e.FileName;
        }

        private string FormatFileName(FileCopyEventArgs e)
        {
            if (string.IsNullOrEmpty(e.FileName))
            {
                return string.Empty;
            }
            if (e.FileName.Length <= 60)
            {
                return e.FileName;
            }

            var returnValue = new StringBuilder();

            string firstPart = e.FileName.Substring(0, 25);
            firstPart = firstPart.Substring(0, firstPart.LastIndexOf("\\"));

            string secondPart = e.FileName.Substring(e.FileName.LastIndexOf("\\"));

            returnValue.Append(firstPart);
            returnValue.Append("(...)");
            returnValue.Append(secondPart);

            return returnValue.ToString();
        }

        private void DisableBackup()
        {

            IsBackupEnabled = false;
        }

        private void EnableBackup()
        {
            IsBackupEnabled = true;
        }

        private bool AreFoldersValid()
        {
            FileBusiness business = new FileBusiness(_environment);

            var validationResult = business.ValidateFolders();

            if (validationResult == null)
            {
                return true;
            }

            _iMessenger.ShowError(validationResult.ErrorMessage);
            return false;
        }

        private void CompareFiles()
        {
            FileBusiness business = new FileBusiness(_environment);

            IEnumerable<ComparisonResult> result = business.CompareFiles();
        }

        private void SaveBackupEnvironment()
        {
            EnvironmentBusiness business = new EnvironmentBusiness();

            business.SaveEnvironmentFolders(_environment);
        }

        private void UpdateFoldersOfBackupEnvironmentReference()
        {
            if (_environment.Folders == null)
                _environment.Folders = new Collection<Folder>();
            else
                _environment.Folders.Clear();

            foreach (var iFolder in Folders)
            {
                _environment.Folders.Add(iFolder);
            }
        }
    }
}
