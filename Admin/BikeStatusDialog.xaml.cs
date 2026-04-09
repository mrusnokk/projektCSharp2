using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Admin
{
    public partial class BikeStatusDialog : Window
    {
        public string NewStatus { get; private set; } = "";
        public string Note { get; private set; } = "";

        public BikeStatusDialog(Bike bike)
        {
            InitializeComponent();
            CurrentStatusText.Text = $"Aktuální stav: {bike.Status}";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (StatusCombo.SelectedItem == null)
            {
                StatusError.Text = "Vyberte nový stav";
                StatusError.Visibility = Visibility.Visible;
                return;
            }
            StatusError.Visibility = Visibility.Collapsed;

            NewStatus = ((ComboBoxItem)StatusCombo.SelectedItem).Tag.ToString()!;
            Note = NoteBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;
    }
}
