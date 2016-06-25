using Heron.Core.Model;
using System;
using System.IO;

namespace Heron.Core.Data {
    
    internal class FilesRepository {

        public event FileCopyHandler OnBackupFile;

        public bool DirectoryExists(string path) {

            return Directory.Exists(path);
        }

        public void CreateDirectoryIfNonExists(string path) {

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public string[] GetAllFiles(string path) {

            return Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
        }

        public int GetTotalFiles(string origin) {

            return GetAllFiles(origin).Length;
        }

        public void CopyDirectoryTo(string origin, string destiny) {

            string[] allFiles = GetAllFiles(origin);
            foreach (var originFile in allFiles) {

                string destinyFile = originFile.Replace(origin, destiny);

                if (destinyFile.Length > 256) {

                    if (OnBackupFile != null) {
                        this.OnBackupFile(this, new FileCopyEventArgs(originFile.ToUpper() + "> 256", destinyFile.ToUpper() + "> 256"));
                    }

                    throw new PathTooLongException(string.Format("File \"{0}\" has a length too long.", destinyFile));
                }
                else {

                    CreateFolder(destinyFile);
                    CopyFile(originFile, destinyFile);
                }
            }
        }

        private void CreateFolder(string destinyFile) {

            string destinyFolder = Path.GetDirectoryName(destinyFile);

            if (!Directory.Exists(destinyFolder))
                Directory.CreateDirectory(destinyFolder);
        }

        private void CopyFile(string originFile, string destinyFile) {

            if (OnBackupFile != null)
                this.OnBackupFile(this, new FileCopyEventArgs(originFile.ToUpper(), destinyFile.ToUpper()));

            if (File.Exists(destinyFile)) {

                FileInfo infoDestiny = new FileInfo(destinyFile);
                FileInfo infoOrigin = new FileInfo(originFile);

                if (infoDestiny.Length != infoOrigin.Length) {
                    File.Copy(originFile, destinyFile, true);
                }
            }
            else {
                File.Copy(originFile, destinyFile);
            }
        }
    }
}
