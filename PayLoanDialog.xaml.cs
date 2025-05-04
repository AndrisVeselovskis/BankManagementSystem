using System;
using System.Windows;

namespace BankManagementSystem
{
    public partial class PayLoanDialog : Window
    {
        public decimal MaxAmount { get; set; }
        public decimal SelectedAmount { get; private set; }

        private bool isSyncing = false;

        public PayLoanDialog(decimal maxAmount)
        {
            InitializeComponent();
            MaxAmount = maxAmount;

            PaymentSlider.Minimum = 0;
            PaymentSlider.Maximum = (double)MaxAmount;
            PaymentSlider.Value = (double)MaxAmount;

            PaymentTextBox.Text = MaxAmount.ToString("N2");
        }

        private void PaymentSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSyncing) return;
            isSyncing = true;
            PaymentTextBox.Text = ((decimal)e.NewValue).ToString("N2");
            isSyncing = false;
        }

        private void PaymentTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isSyncing) return;
            isSyncing = true;

            if (decimal.TryParse(PaymentTextBox.Text, out decimal value))
            {
                if (value > MaxAmount)
                    value = MaxAmount;
                else if (value < 0)
                    value = 0;

                PaymentSlider.Value = (double)value;
            }

            isSyncing = false;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(PaymentTextBox.Text, out decimal amount))
            {
                SelectedAmount = Math.Min(amount, MaxAmount);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid payment amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
