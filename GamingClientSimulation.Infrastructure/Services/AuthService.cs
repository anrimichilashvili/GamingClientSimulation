using GamingClientSimulation.Application.Interfaces;
using GamingClientSimulation.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace GamingClientSimulation.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string userName, string password)
        {
            var loginRequest = new LoginRequest
            {
                UserName = userName,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Login failed with status code: {response.StatusCode}");
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return loginResponse?.Token; 
        }
    }
}
