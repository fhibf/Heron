using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Heron.Pages {
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page {

        private ViewModel.MainViewModel _context;

        public MainPage() {
            InitializeComponent();

            this._context = new ViewModel.MainViewModel();
            this.GridBackup.DataContext = this.DataContext = this._context;
        }
    }
}
