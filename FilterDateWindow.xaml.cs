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
using System.Windows.Shapes;

namespace Transaction_Tracker
{
    /// <summary>
    /// Interaction logic for FilterDateWindow.xaml
    /// </summary>
    public partial class FilterDateWindow : Window
    {
        public DateTime? toDate;
        public DateTime? fromDate;
        public FilterDateWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            toDate = ToPicker.SelectedDate;
            fromDate = FromPicker.SelectedDate;

            DialogResult = true;
        }
    }
}
