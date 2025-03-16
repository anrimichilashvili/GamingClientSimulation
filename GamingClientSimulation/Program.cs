using GamingClientSimulation.Application.Interfaces;
using GamingClientSimulation.Domain.Models;
using GamingClientSimulation.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System.Globalization;
using System.Net.Http.Json;


namespace GamingClientSimulation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var httpClientForHubService = new HttpClient { BaseAddress = new Uri("https://localhost:7012") };
            var httpClientForGameService = new HttpClient { BaseAddress = new Uri("https://localhost:7109") };

            IAuthService authService = new AuthService(httpClientForHubService);

            Console.Write("Enter username: ");
            var userName = Console.ReadLine();
            Console.Write("Enter password: ");
            var password = Console.ReadLine();

            string token;
            try
            {
                token = await authService.LoginAsync(userName, password);
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Login failed. No token returned.");
                    return;
                }
                Console.WriteLine("Login successful!");
                Console.WriteLine($"Your token is: {token}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                return;
            }

            httpClientForGameService.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(token);

            Console.WriteLine("Simulating a game round...");

            Console.Write("Enter Bet Quantity In Double: ");
            var betQuantityString = Console.ReadLine();
            if (double.TryParse(betQuantityString, NumberStyles.Any, CultureInfo.InvariantCulture, out double betQuantity))
            {
                var gameResponse = await httpClientForGameService.PostAsync($"api/GameRound/Play?betAmount={betQuantity}", null);
                if (gameResponse.IsSuccessStatusCode)
                {
                    var gameResultDto = await gameResponse.Content.ReadFromJsonAsync<GameResultDto>();
                    if (gameResultDto.IsWin)
                        Console.WriteLine($"Game round result you win");
                    else
                        Console.WriteLine($"Game round result you loose: {gameResultDto.Amount}");
                }
                else
                {
                    Console.WriteLine($"Game round failed with status code: {gameResponse.StatusCode}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input");
            }


            var hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7012/notificationHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        }
                        return handler;
                    };
                })
                .Build();

            hubConnection.On<string>("PrizeNotification", (message) =>
            {
                Console.WriteLine($"[SignalR] Received prize notification: {message}");
            });

            try
            {
                await hubConnection.StartAsync();
                Console.WriteLine("Connected to SignalR hub. Listening for notifications...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection failed: {ex.Message}");
            }



            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
