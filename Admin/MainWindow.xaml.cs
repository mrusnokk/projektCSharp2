using Admin.Services;
using Microsoft.Win32;
using Shared.Models;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Admin
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _api;

        public MainWindow()
        {
            InitializeComponent();
            _api = App.ApiService!;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
            await LoadBikes();
            await LoadStations();
            await LoadRentals();
        }

        // USERS
        private async void RefreshUsers_Click(object sender, RoutedEventArgs e) =>
            await LoadUsers();

        private async Task LoadUsers()
        {
            try
            {
                var users = await _api.GetUsersAsync();
                UsersGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba načítání uživatelů: {ex.Message}");
            }
        }
        private async void ToggleUserActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var users = UsersGrid.ItemsSource as List<User>;
                var user = users?.FirstOrDefault(u => u.Id == id);
                if (user == null) return;

                try
                {
                    await _api.SetUserActiveAsync(id, !user.IsActive);
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba: {ex.Message}");
                }
            }
        }

        // BIKES
        private async void RefreshBikes_Click(object sender, RoutedEventArgs e) =>
            await LoadBikes();

        private async Task LoadBikes()
        {
            try
            {
                var bikes = await _api.GetBikesAsync();
                BikesGrid.ItemsSource = bikes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba načítání kol: {ex.Message}");
            }
        }

        private async void BikesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BikesGrid.SelectedItem is Bike bike)
            {
                try
                {
                    var history = await _api.GetBikeHistoryAsync(bike.Id);
                    BikeHistoryGrid.ItemsSource = history;
                }
                catch { }
            }
        }

        private void AddBike_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BikeDialog();
            if (dialog.ShowDialog() == true)
                _ = LoadBikes();
        }

        private void EditBike_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var bikes = BikesGrid.ItemsSource as List<Bike>;
                var bike = bikes?.FirstOrDefault(b => b.Id == id);
                if (bike == null) return;

                var dialog = new BikeDialog(bike);
                if (dialog.ShowDialog() == true)
                    _ = LoadBikes();
            }
        }

        private async void ChangeBikeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var bikes = BikesGrid.ItemsSource as List<Bike>;
                var bike = bikes?.FirstOrDefault(b => b.Id == id);
                if (bike == null) return;

                var dialog = new BikeStatusDialog(bike);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        await _api.UpdateBikeStatusAsync(id, dialog.NewStatus, dialog.Note);
                        await LoadBikes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba: {ex.Message}");
                    }
                }
            }
        }

        // STATIONS
        private async void RefreshStations_Click(object sender, RoutedEventArgs e) =>
            await LoadStations();

        private async Task LoadStations()
        {
            try
            {
                var stations = await _api.GetStationsAsync();
                StationsGrid.ItemsSource = stations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba načítání stanovišť: {ex.Message}");
            }
        }

        private void AddStation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new StationDialog();
            if (dialog.ShowDialog() == true)
                _ = LoadStations();
        }

        private void EditStation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var stations = StationsGrid.ItemsSource as List<Station>;
                var station = stations?.FirstOrDefault(s => s.Id == id);
                if (station == null) return;

                var dialog = new StationDialog(station);
                if (dialog.ShowDialog() == true)
                    _ = LoadStations();
            }
        }

        // RENTALS
        private async void RefreshRentals_Click(object sender, RoutedEventArgs e) =>
            await LoadRentals();

        private async Task LoadRentals()
        {
            try
            {
                var rentals = await _api.GetRentalsAsync();
                RentalsGrid.ItemsSource = rentals;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba načítání půjčení: {ex.Message}");
            }
        }
    }
}
