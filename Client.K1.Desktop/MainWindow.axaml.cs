using Avalonia.Controls;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Client.K1.Desktop;

public partial class MainWindow : Window
{
    private readonly HttpClient _http = new();

    public MainWindow()
    {
        InitializeComponent();

        // default headeri, ovo je bilo dok nije bilo hmac pa sam ostavio otkud znam
        _http.DefaultRequestHeaders.Add("X-Client-Id", AppSettings.ClientId);
        _http.DefaultRequestHeaders.Add("X-API-Key", AppSettings.ApiKey);
    }

    // salje potpisani zahtev
    private async Task<HttpResponseMessage> SendSignedAsync(HttpMethod method, string url, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, url) { Content = content };
        // hmac potpisivanje
        await HmacSigner.SignAsync(req, AppSettings.ApiKey);
        return await _http.SendAsync(req);
    }

    private async Task Send(string cmd)
    {
        try
        {
            var url = $"{AppSettings.BaseUrl}/api/commands";
            var payload = new { clientId = AppSettings.ClientId, command = cmd };
            var res = await SendSignedAsync(HttpMethod.Post, url, JsonContent.Create(payload));

            StatusText.Text = $"Status: {(int)res.StatusCode} {res.ReasonPhrase}";
        }
        catch (System.Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private async Task Refresh()
    {
        try
        {
            var url = $"{AppSettings.BaseUrl}/api/state";
            var res = await SendSignedAsync(HttpMethod.Get, url);
            res.EnsureSuccessStatusCode();

            var st = await res.Content.ReadFromJsonAsync<StateDto>();
            StateText.Text = $"State: X={st!.X}, Y={st.Y}, Rot={st.Rot}";
        }
        catch (System.Exception ex)
        {
            StateText.Text = $"State error: {ex.Message}";
        }
    }

    private async void Left_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)   => await Send("Left");
    private async void Right_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)  => await Send("Right");
    private async void Up_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)     => await Send("Up");
    private async void Down_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)   => await Send("Down");
    private async void Rotate_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => await Send("Rotate");
    private async void Refresh_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)=> await Refresh();
}

public record StateDto(int X, int Y, int Rot);