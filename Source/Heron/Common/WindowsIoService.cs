using Heron.Core.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Heron.Common {
    internal class WindowsIoService : IIOService {

        public string GetFolderPath() {

            string returnValue = string.Empty;

            using (var folderDialog = new FolderBrowserDialog()) {

                var result = folderDialog.ShowDialog();

                if (result == DialogResult.OK)
                    returnValue = folderDialog.SelectedPath;
            }

            return returnValue;
        }

        public string GetFileName(string imagePath) {

            return Path.GetFileName(imagePath);
        }

        public bool FolderExists(string folderPath) {

            return Directory.Exists(folderPath);
        }

        public bool CreateFolder(string folderPath) {

            if (!FolderExists(folderPath))
                return Directory.CreateDirectory(folderPath).Exists;
            else
                return false;
        }

        public string[] GetFiles(string folderPath, string extensions) {

            string[] extensionsToVerify = extensions.Split(new char[] { '|' });
            List<string> files = new List<string>(extensionsToVerify.Length);

            foreach (var extension in extensionsToVerify) {

                files.AddRange(Directory.GetFiles(folderPath.Trim(),
                                                  extension,
                                                  SearchOption.TopDirectoryOnly));
            }

            return files.ToArray();
        }
    }
}
