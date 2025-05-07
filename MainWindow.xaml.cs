using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
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

namespace Transaction_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Transactions transactions = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnNewClick(Object sender, RoutedEventArgs e) 
        {
            transactions = new Transactions();
        }

        public void OnOpenClick(Object sender, RoutedEventArgs e)
        {
            transactions = new Transactions();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Transaction Tracker File (*.trantrac)|*.trantrac";
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName == "")
                {
                    MessageBox.Show("Error, user cancelled save");
                    return;
                }
                transactions.Deserialize(openFileDialog.FileName);
            }
        }

        public void OnSaveClick(Object sender, RoutedEventArgs e) 
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".trantrac",
                Filter = "Trantrac Files (*.trantrac)|*.trantrac",
                AddExtension = true,
                OverwritePrompt = true,
                CheckFileExists = false,
            };

            bool? result = saveFileDialog.ShowDialog();
            if (!File.Exists(saveFileDialog.FileName))
            {
                File.WriteAllText(saveFileDialog.FileName, "");
            }

            transactions.Serialize(saveFileDialog.FileName);
        }

        public void AddTransactionClick(Object sender, RoutedEventArgs e) 
        {
            if (transactions == null) {
                MessageBox.Show("No budget selected. Please open an existing or create a new budget");
                return;
            }

            AddTransaction newTransactionWindow = new AddTransaction();
            bool? result = newTransactionWindow.ShowDialog();

            if (result == true) {
                transactions.AddSingleTransaction(newTransactionWindow.CreatedTransaction);
            }
        }
    }
}
