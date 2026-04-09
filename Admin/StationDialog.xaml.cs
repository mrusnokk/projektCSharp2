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
    public partial class StationDialog : Window
    {
        private readonly Station? _existing;

        public StationDialog(Station? station = null)
        {
            InitializeComponent();
            _existing = station;

            if (station != null)
            {
                NameBox.Text = station.Name;
                AddressBox.Text = station.Address;
                LatBox.Text = station.Lat.ToString();
                LngBox.Text = station.Lng.ToString();
                CapacityBox.Text = station.Capacity.ToString();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                NameError.Text = "Název je povinný";
                NameError.Visibility = Visibility.Visible;
                valid = false;
            }
            else NameError.Visibility = Visibility.Collapsed;

            if (!double.TryParse(LatBox.Text, out double lat))
            {
                LatError.Text = "Neplatná hodnota latitude";
                LatError.Visibility = Visibility.Visible;
                valid = false;
            }
            else LatError.Visibility = Visibility.Collapsed;

            if (!double.TryParse(LngBox.Text, out double lng))
            {
                LngError.Text = "Neplatná hodnota longitude";
                LngError.Visibility = Visibility.Visible;
                valid = false;
            }
            else LngError.Visibility = Visibility.Collapsed;

            if (!int.TryParse(CapacityBox.Text, out int capacity) || capacity <= 0)
            {
                CapacityError.Text = "Kapacita musí být kladné číslo";
                CapacityError.Visibility = Visibility.Visible;
                valid = false;
            }
            else CapacityError.Visibility = Visibility.Collapsed;

            if (!valid) return;

            try
            {
                var station = new Station
                {
                    Name = NameBox.Text.Trim(),
                    Address = AddressBox.Text.Trim(),
                    Lat = (decimal)lat,
                    Lng = (decimal)lng,
                    Capacity = capacity,
                    IsActive = _existing?.IsActive ?? true
                };

                if (_existing == null)
                    await App.ApiService!.CreateStationAsync(station);
                else
                    await App.ApiService!.UpdateStationAsync(_existing.Id, station);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při ukládání: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;
    }
}
