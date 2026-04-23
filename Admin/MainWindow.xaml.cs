using Admin.Services;
using Microsoft.Win32;
using Shared.Models;
using System.ComponentModel;
using System.Net.Http;
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
        private System.Threading.Timer? _refreshTimer;
        private string _sortBy = "Id";
        private string _sortDir = "ASC";


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

            _refreshTimer = new System.Threading.Timer(async _ =>
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await LoadBikes();
                    await LoadStations();
                });
            }, null, 30000, 30000);
        }
        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Dispose();
            base.OnClosed(e);
        }

        private async void UniversalGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            string propertyName = e.Column.SortMemberPath;
            if (string.IsNullOrEmpty(propertyName)) return;

            if (_sortBy == propertyName)
            {
                _sortDir = _sortDir == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                _sortBy = propertyName;
                _sortDir = "ASC";
            }

            e.Column.SortDirection = _sortDir == "ASC"
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            if (sender == BikesGrid) await LoadBikes();
            else if (sender == UsersGrid) await LoadUsers();
            else if (sender == StationsGrid) await LoadStations();
            else if (sender == RentalsGrid) await LoadRentals();
        }


        // USERS
        private async void RefreshUsers_Click(object sender, RoutedEventArgs e) =>
            await LoadUsers();

        private async Task LoadUsers()
        {
            try
            {
                var users = await _api.GetUsersAsync(_sortBy, _sortDir);
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
                var bikes = await _api.GetBikesAsync(_sortBy, _sortDir);
                BikesGrid.ItemsSource = bikes;

                if (bikes.Any())
                {
                    BikesGrid.SelectedIndex = 0;
                    var history = await _api.GetBikeHistoryAsync(bikes.First().Id);
                    BikeHistoryGrid.ItemsSource = history;
                }
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
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba historie: {ex.Message}");
                }
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
                if (bike.Status == "rented")
                {
                    MessageBox.Show(
                        "Nelze změnit stav půjčeného kola. Kolo musí být nejdříve vráceno.",
                        "Akce není dostupná",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var dialog = new BikeStatusDialog(bike);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        await _api.UpdateBikeStatusAsync(id, dialog.NewStatus, dialog.Note);
                        await LoadBikes();
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("400"))
                    {
                        MessageBox.Show(
                            "Nepodařilo se změnit stav kola. Je pravděpodobné, že si ho mezitím někdo půjčil přes web. Aplikace nyní stáhne aktuální data.",
                            "Zastaralá data",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        try
                        {
                            var freshBikes = await _api.GetBikesAsync();
                            BikesGrid.ItemsSource = freshBikes;
                        }
                        catch (Exception loadEx)
                        {
                            MessageBox.Show($"Chyba při stahování aktuálních dat: {loadEx.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Došlo k chybě při komunikaci se serverem: {ex.Message}",
                            "Chyba",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
        private async void DeleteBike_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var bikes = BikesGrid.ItemsSource as List<Bike>;
                var bike = bikes?.FirstOrDefault(b => b.Id == id);
                if (bike == null) return;

                if (bike.Status == "rented")
                {
                    MessageBox.Show("Nelze smazat půjčené kolo.");
                    return;
                }

                var result = MessageBox.Show(
                    $"Opravdu chcete smazat kolo {bike.Code}?",
                    "Potvrzení mazání",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _api.DeleteBikeAsync(id);
                        await LoadBikes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba při mazání: {ex.Message}");
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
                var stations = await _api.GetStationsAsync(_sortBy, _sortDir);
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
        private async void DeleteStation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var stations = StationsGrid.ItemsSource as List<Station>;
                var station = stations?.FirstOrDefault(s => s.Id == id);
                if (station == null) return;

                var result = MessageBox.Show(
                    $"Opravdu chcete smazat stanoviště {station.Name}?",
                    "Potvrzení mazání",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _api.DeleteStationAsync(id);
                        await LoadStations();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba při mazání: {ex.Message}");
                    }
                }
            }
        }

        // RENTALS
        private async void RefreshRentals_Click(object sender, RoutedEventArgs e) =>
            await LoadRentals();

        private async Task LoadRentals()
        {
            try
            {
                var rentals = await _api.GetRentalsAsync(_sortBy, _sortDir);
                RentalsGrid.ItemsSource = rentals;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba načítání půjčení: {ex.Message}");
            }
        }
    }
}

