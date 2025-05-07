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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AddTransaction : Window
    {
        public Transaction CreatedTransaction;

        public AddTransaction()
        {
            InitializeComponent();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreatedTransaction = new Transaction(
                DateBox.DisplayDate,
                AccountBox.Text,
                PayeeBox.Text,
                DescBox.Text,
                CatBox.Text,
                double.Parse(AmountBox.Text)
                );
            this.DialogResult = true;
        }
    }
}
