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
        //variables for main window to use
        Transactions transactions = null;
        private bool _tracker = false;
        public event Action GraphUpdated = delegate { };
        private GraphingController _graphingController = null;
        Filter transactionFilter = new Filter();

        public MainWindow()
        {
            InitializeComponent();
            //Subscribe to loading event when window is fully loaded
            Loaded += MainWindow_Loaded;
        }
        //asynchronus threaded task to be performed in the background upon window loading
        private Task AwaitInitGraphs()
        {
            //create new graphing controller and initalize graphs
            _graphingController = new GraphingController(TransactionsMoneySpentGraph, TransactionsByCategoryGraph);
            //call update graphs function when graph updated is invoked
            GraphUpdated += UpdateGraphs;
            return Task.CompletedTask;
        }
        //async method to be performed in the background
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //await to init the graphs loading in the background thread
            await AwaitInitGraphs();
        }
        //when new button is clicked, create new transactions object
        public void OnNewClick(Object sender, RoutedEventArgs e) 
        {
            transactions = new Transactions();
        }

        public void OnOpenClick(Object sender, RoutedEventArgs e)
        {
            //when open is clicked, create new transactions 
            transactions = new Transactions();
            //populate open file dialog and look for our custom file extension files
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Transaction Tracker File (*.trantrac)|*.trantrac";
            if (openFileDialog.ShowDialog() == true)
            {
                //error handling in case user desn't select a file
                if (openFileDialog.FileName == "")
                {
                    MessageBox.Show("Error, user cancelled save");
                    return;
                }
                //Deserialize given function
                transactions.Deserialize(openFileDialog.FileName);
            }
            //refresh both graphs by invoking through event and GUI by updating listbox
            Refresh();
            GraphUpdated?.Invoke();
        }
        //when save button is clicked
        public void OnSaveClick(Object sender, RoutedEventArgs e) 
        {
            //error handling if no transactions have been loaded or saved yet
            if (transactions == null) {
                MessageBox.Show("Please load a transaction tracker before saving!");
                return;
            }
            //populate with save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".trantrac",
                Filter = "Trantrac Files (*.trantrac)|*.trantrac",
                AddExtension = true,
                OverwritePrompt = true,
                CheckFileExists = false,
            };
            //if file doesn't already exist, we write to the given filename to create the file then call our serialize function to create given transactions format
            bool? result = saveFileDialog.ShowDialog();
            if (!File.Exists(saveFileDialog.FileName))
            {
                File.WriteAllText(saveFileDialog.FileName, "");
            }

            transactions.Serialize(saveFileDialog.FileName);
        }
        //when you want to add a transaction
        public void AddTransactionClick(Object sender, RoutedEventArgs e) 
        {
            //error handling in case no transaction budget is loaded or started yet.
            if (transactions == null) {
                MessageBox.Show("No budget selected. Please open an existing or create a new budget");
                return;
            }
            //create transaction window
            AddTransaction newTransactionWindow = new AddTransaction();
            bool? result = newTransactionWindow.ShowDialog();

            if (result == true) {
                //add given transaction to list
                transactions.AddSingleTransaction(newTransactionWindow.CreatedTransaction);
            }
            //update graphs and listbox
            Refresh();
            GraphUpdated?.Invoke();
        }
        //if you select something different in the listbox
        public void SelectionChange(Object sender, RoutedEventArgs e) 
        {
            //if the given selection is a transaction object..
            if (TransactionsListBox.SelectedItem is Transaction selection)
            {
                _tracker = true;
                //populate the given fields to show detailed view of transaction on bottom-left of screen
                DateBox.SelectedDate = selection.date;
                AccountBox.Text = selection.account;
                PayeeBox.Text = selection.payee;
                DescriptionBox.Text = selection.description;
                CategoryBox.Text = selection.category;
                AmountBox.Text = selection.amount.ToString("F2");
                RecurringBox.IsChecked = selection.recurring;
                //enable all boxes to show they are in use
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
        //to update graphs, this function is called when event is invoked
        public void UpdateGraphs()
        {
            //if transactions hasn't been created yet, just return since there are no graphs to populate
            if (transactions == null)
            {
                return;
            }
            //add data based on given filter and all transactions
            transactionFilter.AddData(transactions.All);
            //give graphing controller the filter output to generate the graphs
            _graphingController.PopulateGraphs(transactionFilter.GetFilterOutput());

        }
        //refreshes listbox output and detailed view output
        public void Refresh() 
        {
            //error handling when transactions hasn't been created yet.
            if (transactions == null) 
            {
                MessageBox.Show("Please load a file or make a new file");
                return;
            }
            //set all detailed view elements back to empty
            _tracker = true;
            DateBox.SelectedDate = null;
            AccountBox.Text = "";
            PayeeBox.Text = "";
            DescriptionBox.Text = "";
            CategoryBox.Text = "";
            AmountBox.Text = "";
            RecurringBox.IsChecked = false;
            _tracker = false;
            //add data based on filter and update listbox view based on filter
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
        //for predicting textbox text
        private string GetPreviewText(TextBox textBox, string input)
        {
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;

            string text = textBox.Text;
            string newText = text.Remove(selectionStart, selectionLength).Insert(selectionStart, input);
            return newText;
        }
        //making sure that given entry for price is a double with 2 decimal points
        private bool IsValidDoubleWithTwoDecimals(string text)
        {
            //if not a double return false
            if (!double.TryParse(text, out _)) return false;
            //if it has more than two decimal points return false
            int dotIndex = text.IndexOf('.');
            if (dotIndex >= 0 && text.Length - dotIndex - 1 > 2)
                return false;

            return true;
        }
        //updating listbox
        public void BoxUpdate(Object sender, RoutedEventArgs e) 
        {
            //if our main window tracker is not active
            if (_tracker == false)
            {
                //and our given listbox item is a transaction
                if (TransactionsListBox.SelectedItem is Transaction selection)
                {
                    //error handling making sure all fields have a value
                    if (AmountBox.Text == "" ||
                        DateBox.SelectedDate == null ||
                        DescriptionBox.Text == "" ||
                        PayeeBox.Text == "" ||
                        AccountBox.Text == "" ||
                        CategoryBox.Text == "") {
                        MessageBox.Show("No Fields can be empty!");
                        return;
                    }
                    //making sure if edit fields cannot contain commas, as they are the only disallowed characte due to seralize using comma as a separator
                    if ((DescriptionBox.Text + PayeeBox.Text + AccountBox.Text + CategoryBox.Text).Contains(",")) 
                    {
                        MessageBox.Show("Commas are not allowed in text fields. Please update immedietly");
                        return;
                    }
                    //otherwise we'll edit based on the selections
                    selection.amount = double.Parse(AmountBox.Text);
                    selection.date =  (DateTime) DateBox.SelectedDate;
                    selection.description = DescriptionBox.Text;
                    selection.payee = PayeeBox.Text;
                    selection.account = AccountBox.Text;
                    selection.category = CategoryBox.Text;
                    selection.recurring = RecurringBox.IsChecked ?? false;
                    CollectionViewSource.GetDefaultView(TransactionsListBox.ItemsSource).Refresh();
                    //and update graphs
                    GraphUpdated?.Invoke();
                }
            }
        }

        public void DeleteClick(Object sender, RoutedEventArgs e) 
        {
            //error handling on delete
            if (transactions == null) 
            {
                MessageBox.Show("Please load a file or make a new file");
                return;
            }
            //remove given transaction and refresh/update
            transactions.Remove(TransactionsListBox.SelectedItem as Transaction);
            Refresh();
            GraphUpdated?.Invoke();
        }

        public void ResetFilters(Object sender, RoutedEventArgs e) 
        {
            //create new filter and update/refresh
            transactionFilter = new Filter();
            Refresh();
            GraphUpdated?.Invoke();
        }

        public void CategorySearchClick(Object sender, RoutedEventArgs e) 
        {
            //create new category search window and apply its updates through refresh/filter
            CategorySearch window = new CategorySearch();
            bool? result = window.ShowDialog();

            if (result == true)
            {
                transactionFilter = new CategoryFilter(window.searchContent);
                Refresh();
                GraphUpdated?.Invoke();
            }
        }

        public void DescriptionSearchClick(Object sender, RoutedEventArgs e) 
        {
            //create new description search window and apply its updates through refresh/filter
            DescriptionSearch window = new DescriptionSearch();
            bool? result = window.ShowDialog();

            if (result == true)
            {
                transactionFilter = new DescriptionFilter(window.searchContent);
                Refresh();
                GraphUpdated?.Invoke();
            }
        }

        public void AddDateFilter(Object sender, RoutedEventArgs e) 
        {
            //create new date search window and apply its updates through refresh/filter
            FilterDateWindow window = new FilterDateWindow();
            bool? result = window.ShowDialog();

            if (result == true) 
            {
                if (window.toDate != null) { transactionFilter.ToDate((DateTime)window.toDate); }
                if (window.fromDate != null) { transactionFilter.FromDate((DateTime)window.fromDate); }
                Refresh();
                GraphUpdated?.Invoke();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //on exit shutdown the application
            Application.Current.Shutdown();
        }
    }
}