using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heron.Core.Model {
    public class Folder {

        public Guid Id { get; set; }

        public string Path { get; set; }

        public Folder(Guid id, string path) {

            this.Id = id;
            this.Path = path;
        }

        public Folder() {

            this.Id = Guid.Empty;
        }
    }
}
