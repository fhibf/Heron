using Heron.Core.Model;
using System.Windows.Forms;

namespace Heron.Common {
    internal class WindowsMessenger : IMessengerService {

        private const string DEFAULT_CAPTION = "Heron";

        public void ShowInformation(string message) {

            MessageBox.Show(message, DEFAULT_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowWarning(string message) {

            MessageBox.Show(message, DEFAULT_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string message) {

            MessageBox.Show(message, DEFAULT_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
