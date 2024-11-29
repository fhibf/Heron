using Heron.Core.Model;
using System.IO;

namespace Heron.Core.Data
{

    internal class FilesRepository
    {
        public event FileCopyHandler OnBackupFile;

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectoryIfNonExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string[] GetAllFiles(string path)
        {
            return Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
        }

        public int GetTotalFiles(string origin)
        {
            return GetAllFiles(origin).Length;
        }

        public void CopyDirectoryTo(string origin, string destiny)
        {
            string[] allFiles = GetAllFiles(origin);
            foreach (var originFile in allFiles)
            {
                string destinyFile = originFile.Replace(origin, destiny);

                // Maximum Path Length Limitation
                // https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=registry
                if (destinyFile.Length > 256)
                {
                    destinyFile = "\\\\?\\" + destinyFile;
                }

                CreateFolder(destinyFile);
                CopyFile(originFile, destinyFile);
            }
        }

        private void CreateFolder(string destinyFile)
        {
            string destinyFolder = Path.GetDirectoryName(destinyFile);

            if (!Directory.Exists(destinyFolder))
                Directory.CreateDirectory(destinyFolder);
        }

        private void CopyFile(string originFile, string destinyFile)
        {
            if (OnBackupFile != null)
            {
                this.OnBackupFile(this, new FileCopyEventArgs(originFile.ToUpper(), destinyFile.ToUpper()));
            }

            if (File.Exists(destinyFile))
            {
                FileInfo infoDestiny = new FileInfo(destinyFile);
                FileInfo infoOrigin = new FileInfo(originFile);

                if (infoDestiny.Length != infoOrigin.Length)
                {
                    File.Copy(originFile, destinyFile, true);
                }
            }
            else
            {
                File.Copy(originFile, destinyFile);
            }
        }
    }
}
