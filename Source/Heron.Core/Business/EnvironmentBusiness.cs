using Heron.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heron.Core.Business {
    public class EnvironmentBusiness {
        
        public BackupEnvironment GetCurrentBackupEnvironment() {

            var repository = new Data.EnvironmentRepository();

            var environments = repository.GetAllBackupEnvironments();

            var currentEnvironment = IdentifyCurrentEnvironment(environments);

            if (currentEnvironment == null)
                currentEnvironment = repository.CreateNewBackupEnvironment(GetEnvironmentName());
            
            return currentEnvironment;
        }

        private static string GetEnvironmentName() {

            return Environment.MachineName;
        }

        public void SaveEnvironmentFolders(BackupEnvironment environment) {

            var repository = new Data.EnvironmentRepository();

            repository.SaveEnvironmentFolders(environment);
        }

        private static BackupEnvironment IdentifyCurrentEnvironment(IEnumerable<BackupEnvironment> environments) {

            if (environments == null)
                return null;

            string environmentName = GetEnvironmentName();

            var currentEnvironment = environments.FirstOrDefault(e => string.Equals(e.Name, environmentName,
                                                                 StringComparison.CurrentCultureIgnoreCase));

            return currentEnvironment;
        }
    }
}
