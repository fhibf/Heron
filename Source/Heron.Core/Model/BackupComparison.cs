using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heron.Core.Model {
    public class BackupComparison {

        public string Origin { get; set; }

        public string Destiny { get; set; }

        public ComparisonResult Comparison { get; set; }
    }
}
