using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heron.Core.Model {
    public class BackupEnvironment {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime? LastBackup { get; set; }

        public ICollection<Folder> Folders { get; set; }

        public BackupEnvironment() {

            this.Id = Guid.Empty;
            this.Folders = new Collection<Folder>();
        }
    }
}
