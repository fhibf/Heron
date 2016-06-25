using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heron.Core.Model {

    public delegate void FileCopyHandler(object sender, FileCopyEventArgs e);

    public class FileCopyEventArgs : EventArgs {

        public string FileName { get; set; }

        public string DestinyFileName { get; set; }

        public FileCopyEventArgs(string fileName, string destinyFileName) {

            this.FileName = fileName;
            this.DestinyFileName = destinyFileName;
        }
    }
}
