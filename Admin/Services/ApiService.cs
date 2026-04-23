using Newtonsoft.Json;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Admin.Models;

namespace Admin.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5045";
        private const string AdminToken = "ADMIN_DESKTOP_TOKEN";

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri(BaseUrl);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AdminToken);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task PostAsync<T>(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        private async Task PutAsync(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<T> PutAsync<T>(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseJson);
        }
        

        // Stations
        public async Task<List<Station>> GetStationsAsync(string sortBy = "Id", string sortDir = "ASC") =>
            await GetAsync<List<Station>>($"/api/stations?sortBy={sortBy}&sortDir={sortDir}");
        public async Task<StationDetail> GetStationAsync(int id) =>
            await GetAsync<StationDetail>($"/api/stations/{id}");
        public async Task CreateStationAsync(Station station) =>
        await PostAsync<object>("/api/stations", station);

        public async Task UpdateStationAsync(int id, Station station) =>
            await PutAsync($"/api/stations/{id}", station);
        public async Task DeleteStationAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/stations/{id}");
            response.EnsureSuccessStatusCode();
        }


        // Bikes
        public async Task<List<Bike>> GetBikesAsync(string sortBy = "Id", string sortDir = "ASC") =>
            await GetAsync<List<Bike>>($"/api/bikes?sortBy={sortBy}&sortDir={sortDir}");

        public async Task<Bike> GetBikeAsync(int id) =>
            await GetAsync<Bike>($"/api/bikes/{id}");

        public async Task CreateBikeAsync(Bike bike) =>
            await PostAsync<object>("/api/bikes", bike);

        public async Task UpdateBikeAsync(int id, Bike bike) =>
            await PutAsync($"/api/bikes/{id}", bike);

        public async Task UpdateBikeStatusAsync(int id, string newStatus, string note) =>
            await PutAsync($"/api/bikes/{id}/status", new { newStatus, note });

        public async Task<List<BikeStatusHistory>> GetBikeHistoryAsync(int id) =>
            await GetAsync<List<BikeStatusHistory>>($"/api/bikes/{id}/history");

        // Users
        public async Task<List<User>> GetUsersAsync(string sortBy = "Id", string sortDir = "ASC") =>
            await GetAsync<List<User>>($"/api/users?sortBy={sortBy}&sortDir={sortDir}");

        public async Task SetUserActiveAsync(int id, bool isActive) =>
            await PutAsync($"/api/users/{id}/active", new { isActive });
        public async Task DeleteUserAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/users/{id}");
            response.EnsureSuccessStatusCode();
        }

        // Rentals
        public async Task<List<Rental>> GetRentalsAsync(string sortBy="StartedAt", string sortDir = "ASC") =>
            await GetAsync<List<Rental>>($"/api/rentals?sortBy={sortBy}&sortDir={sortDir}");
        public async Task DeleteBikeAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/bikes/{id}");
            response.EnsureSuccessStatusCode();
        }

        

        
    }
}
