using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public partial class BikeDialog : Window
    {
        private readonly Bike? _existing;

        public BikeDialog(Bike? bike = null)
        {
            InitializeComponent();
            _existing = bike;

            if (bike != null)
            {
                CodeBox.Text = bike.Code;
                ModelBox.Text = bike.Model;
                StationBox.Text = bike.CurrentStationId?.ToString() ?? "";
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validace
            bool valid = true;

            if (string.IsNullOrWhiteSpace(CodeBox.Text))
            {
                CodeError.Text = "Kód kola je povinný";
                CodeError.Visibility = Visibility.Visible;
                valid = false;
            }
            else CodeError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(ModelBox.Text))
            {
                ModelError.Text = "Model je povinný";
                ModelError.Visibility = Visibility.Visible;
                valid = false;
            }
            else ModelError.Visibility = Visibility.Collapsed;

            int? stationId = null;
            if (!string.IsNullOrWhiteSpace(StationBox.Text))
            {
                if (!int.TryParse(StationBox.Text, out int sid))
                {
                    StationError.Text = "ID stanoviště musí být číslo";
                    StationError.Visibility = Visibility.Visible;
                    valid = false;
                }
                else
                {
                    stationId = sid;
                    StationError.Visibility = Visibility.Collapsed;
                }
            }
            else StationError.Visibility = Visibility.Collapsed;

            if (!valid) return;

            try
            {
                var bike = new Bike
                {
                    Code = CodeBox.Text.Trim(),
                    Model = ModelBox.Text.Trim(),
                    CurrentStationId = stationId,
                    Status = _existing?.Status ?? "available"
                };

                if (_existing == null)
                    await App.ApiService!.CreateBikeAsync(bike);
                else
                    await App.ApiService!.UpdateBikeAsync(_existing.Id, bike);

                DialogResult = true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                CodeError.Text = "Kolo s tímto kódem již existuje";
                CodeError.Visibility = Visibility.Visible;
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
