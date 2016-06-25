using Heron.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Heron.Core.Data {
    internal class EnvironmentRepository {

        public IEnumerable<BackupEnvironment> GetAllBackupEnvironments() {

            string file = GetDataPath();

            using (var fileStr = new FileStream(file, FileMode.Open)) {

                var dataDoc = XDocument.Load(fileStr);
                 
                var elementsEnvironment = dataDoc.Descendants("environment");

                foreach (var element in elementsEnvironment) {

                    BackupEnvironment newEnvironment = new BackupEnvironment();

                    newEnvironment.Name = element.Attribute("name").Value;                    
                    newEnvironment.Id = Guid.Parse(element.Attribute("id").Value);

                    string lastBackupStr = element.Attribute("lastBackup").Value;
                    DateTime lastBackup = DateTime.MinValue;
                    if (DateTime.TryParse(lastBackupStr, out lastBackup))
                        newEnvironment.LastBackup = lastBackup;
                    else
                        newEnvironment.LastBackup = DateTime.MinValue;

                    var folders = element.Descendants("folder");
                    if (folders != null && folders.Count() > 0) {

                        foreach (var elementFolder in folders) {

                            Folder newFolder = new Folder() {
                                Id = new Guid(elementFolder.Attribute("id").Value),
                                Path = elementFolder.Attribute("path").Value
                            };

                            newEnvironment.Folders.Add(newFolder);
                        }
                    }

                    yield return newEnvironment;
                }
            }
        }
        
        public BackupEnvironment CreateNewBackupEnvironment(string environmentName) {

            var returnValue = new BackupEnvironment() { Id = Guid.NewGuid(), Name = environmentName };

            SaveNewEnvironment(returnValue);

            return returnValue;
        }

        public void SaveEnvironmentFolders(BackupEnvironment environment) {

            string file = GetDataPath();

            var dataDoc = XDocument.Load(file);

            var rootElement = dataDoc.Root;

            var currentEnvironmentElement = rootElement.Descendants("environment").SingleOrDefault(e => new Guid(e.Attribute("id").Value) == environment.Id);

            if (currentEnvironmentElement == null)
                throw new InvalidOperationException("Cannot found any environment with id " + environment.Id + ".");

            // Delete all child folder elements
            if (currentEnvironmentElement.Element("folders") != null)
                currentEnvironmentElement.Element("folders").Remove();
            // Create root folders' element
            var rootFoldersElement = new XElement("folders");
            // Iterate over folders and add all of them to root folders' element
            foreach (var iFolder in environment.Folders) {

                var folderElement = new XElement("folder");
                folderElement.SetAttributeValue("id", iFolder.Id);
                folderElement.SetAttributeValue("path", iFolder.Path);

                rootFoldersElement.Add(folderElement);
            }

            currentEnvironmentElement.Add(rootFoldersElement);

            dataDoc.Save(file);
        }

        private static void SaveNewEnvironment(BackupEnvironment environment) {

            var newElement = new XElement("environment");

            newElement.SetAttributeValue("name", environment.Name);
            newElement.SetAttributeValue("id", environment.Id);
            newElement.SetAttributeValue("lastBackup", DateTime.MinValue);

            string file = GetDataPath();

            var dataDoc = XDocument.Load(file);

            var rootElement = dataDoc.Root;

            rootElement.Add(newElement);

            dataDoc.Save(file);            
        }

        private static string GetDataPath() {

            string dataPath = Path.Combine(Environment.CurrentDirectory, "data.xml");

            if (!File.Exists(dataPath))
                CreateDataFile(dataPath);
            
            return dataPath;
        }

        private static void CreateDataFile(string path) {

            XDocument document = new XDocument(new XElement("backups"));

            document.Save(path);
        }
        
        public void SaveBackupDate(BackupEnvironment environment) {

            string file = GetDataPath();

            var dataDoc = XDocument.Load(file);

            var rootElement = dataDoc.Root;

            var currentEnvironmentElement = rootElement.Descendants("environment").SingleOrDefault(e => new Guid(e.Attribute("id").Value) == environment.Id);

            if (currentEnvironmentElement == null)
                throw new InvalidOperationException("Cannot found any environment with id " + environment.Id + ".");

            currentEnvironmentElement.SetAttributeValue("lastBackup", environment.LastBackup.Value);

            dataDoc.Save(file);
        }
    }
}
