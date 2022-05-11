using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace StockAnalyzer.Windows
{
    public partial class MainWindow : Window
    {
        CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (IsSearchInProgress())
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

                Search.Content = "Search";
                return;
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationTokenSource.Token.Register(() => Status.Text = "Cancellation requested");
                Search.Content = "Cancel";

                Progress.Visibility = Visibility.Visible;
                Progress.IsIndeterminate = true;

                Data.ItemsSource = await GetAccountData(Identifier.Text, _cancellationTokenSource.Token);

                Status.Text = $"Loaded data for {Identifier.Text}";
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
            finally
            {
                Progress.Visibility = Visibility.Hidden;

                _cancellationTokenSource = null;
                Search.Content = "Search";
            }
        }

        private bool IsSearchInProgress()
        {
            return _cancellationTokenSource != null;
        }

        private async Task<Dictionary<string, string>> GetAccountData(string identifier, CancellationToken cancellationToken)
        {
            const string API_URL = "https://api.github.com/users";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                var result = await client.GetAsync($"{API_URL}/{identifier}", cancellationToken);

                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();

                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                return dict;
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Identifier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, e);
            }
        }
    }
}
