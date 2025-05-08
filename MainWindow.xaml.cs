using Microsoft.Win32;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Configuration;
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
        private bool _tracker = false;
        private readonly Stopwatch _graphUpdateStopwatch = new Stopwatch();
        public event Action GraphUpdated = delegate { };
        private GraphingController _graphingController = null;

        Filter transactionFilter = new Filter();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private Task AwaitInitGraphs()
        {
            _graphingController = new GraphingController(TransactionsMoneySpentGraph, TransactionsByCategoryGraph);
            _graphUpdateStopwatch.Start();
            GraphUpdated += UpdateGraphs;
            return Task.CompletedTask;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await AwaitInitGraphs();
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
            Refresh();
            GraphUpdated?.Invoke();
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
            Refresh();
            GraphUpdated?.Invoke();
        }

        public void SelectionChange(Object sender, RoutedEventArgs e) 
        {
            if (TransactionsListBox.SelectedItem is Transaction selection)
            {
                _tracker = true;

                DateBox.SelectedDate = selection.date;
                AccountBox.Text = selection.account;
                PayeeBox.Text = selection.payee;
                DescriptionBox.Text = selection.description;
                CategoryBox.Text = selection.category;
                AmountBox.Text = selection.amount.ToString("F2");
                RecurringBox.IsChecked = selection.recurring;

                DateBox.IsEnabled = true;
                AccountBox.IsEnabled = true;
                PayeeBox.IsEnabled = true;
                DescriptionBox.IsEnabled = true;
                CategoryBox.IsEnabled = true;
                AmountBox.IsEnabled = true;
                RecurringBox.IsEnabled = true;
                _tracker = false;
            }
        }
        public void UpdateGraphs()
        {
            _graphingController.PopulateGraphs(transactions.All);

        }
        public void Refresh() 
        {
            _tracker = true;
            DateBox.SelectedDate = null;
            AccountBox.Text = "";
            PayeeBox.Text = "";
            DescriptionBox.Text = "";
            CategoryBox.Text = "";
            AmountBox.Text = "";
            RecurringBox.IsChecked = false;
            _tracker = false;

            transactionFilter.AddData(transactions.All);
            TransactionsListBox.ItemsSource = transactionFilter.GetFilterOutput().ToList();
        }
        
        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;

            // Predict what the full text will be after input
            string fullText = GetPreviewText(textBox, e.Text);

            // Validate full input as a double and limit to 2 decimal places
            e.Handled = !IsValidDoubleWithTwoDecimals(fullText);
        }

        private string GetPreviewText(TextBox textBox, string input)
        {
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;

            string text = textBox.Text;
            string newText = text.Remove(selectionStart, selectionLength).Insert(selectionStart, input);
            return newText;
        }

        private bool IsValidDoubleWithTwoDecimals(string text)
        {
            if (!double.TryParse(text, out _)) return false;

            int dotIndex = text.IndexOf('.');
            if (dotIndex >= 0 && text.Length - dotIndex - 1 > 2)
                return false;

            return true;
        }

        public void BoxUpdate(Object sender, RoutedEventArgs e) 
        {
            if (_tracker == false)
            {
                if (TransactionsListBox.SelectedItem is Transaction selection)
                {
                    selection.amount = double.Parse(AmountBox.Text);
                    selection.date =  (DateTime) DateBox.SelectedDate;
                    selection.description = DescriptionBox.Text;
                    selection.payee = PayeeBox.Text;
                    selection.account = AccountBox.Text;
                    selection.category = CategoryBox.Text;
                    selection.recurring = RecurringBox.IsChecked ?? false;
                    CollectionViewSource.GetDefaultView(TransactionsListBox.ItemsSource).Refresh();
                    GraphUpdated?.Invoke();
                }
            }
        }

        public void DeleteClick(Object sender, RoutedEventArgs e) 
        {
            transactions.Remove(TransactionsListBox.SelectedItem as Transaction);
            Refresh();
            GraphUpdated?.Invoke();
        }

        public void ResetFilters(Object sender, RoutedEventArgs e) 
        {
            transactionFilter = new Filter();
            Refresh();
        }

        public void CategorySearchClick(Object sender, RoutedEventArgs e) 
        {
            CategorySearch window = new CategorySearch();
            bool? result = window.ShowDialog();

            if (result == true)
            {
                transactionFilter = new CategoryFilter(window.searchContent);
                Refresh();
            }
        }

        public void DescriptionSearchClick(Object sender, RoutedEventArgs e) 
        {
            DescriptionSearch window = new DescriptionSearch();
            bool? result = window.ShowDialog();

            if (result == true)
            {
                transactionFilter = new DescriptionFilter(window.searchContent);
                Refresh();
            }
        }

        public void AddDateFilter(Object sender, RoutedEventArgs e) 
        {
            FilterDateWindow window = new FilterDateWindow();
            bool? result = window.ShowDialog();

            if (result == true) 
            {
                if (window.toDate != null) { transactionFilter.ToDate((DateTime)window.toDate); }
                if (window.fromDate != null) { transactionFilter.FromDate((DateTime)window.fromDate); }
                Refresh();
            }
        }

    }
}