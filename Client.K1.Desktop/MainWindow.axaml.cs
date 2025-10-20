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
        _http.DefaultRequestHeaders.Add("X-Client-Id", AppSettings.ClientId);
        _http.DefaultRequestHeaders.Add("X-API-Key", AppSettings.ApiKey);
    }

    private async Task Send(string cmd)
    {
        try
        {
            var res = await _http.PostAsJsonAsync($"{AppSettings.BaseUrl}/api/commands",
                new { clientId = AppSettings.ClientId, command = cmd });
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
            var st = await _http.GetFromJsonAsync<StateDto>($"{AppSettings.BaseUrl}/api/state");
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