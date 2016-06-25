using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Heron {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {

            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            this.Title = string.Format("Heron - {0}", version);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {

            this.TransitionControl.Navigate(new Pages.MainPage());
        }
    }
}
