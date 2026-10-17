using System.Threading.Tasks;
using System.Windows;

namespace CommerX.Desktop;

public partial class MainWindow : Window
{
    private readonly DatabaseHealthCheck _healthCheck;

    // el contenedor inyecta la dependencia: la ventana no sabe cómo se comprueba la base
    public MainWindow(DatabaseHealthCheck healthCheck)
    {
        _healthCheck = healthCheck;
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) => await CheckAsync();

    private async void OnCheckConnection(object sender, RoutedEventArgs e) => await CheckAsync();

    private async Task CheckAsync()
    {
        StatusText.Text = "Base de datos: comprobando...";

        var connected = await _healthCheck.CanConnectAsync();

        StatusText.Text = connected
            ? "Base de datos: conectada"
            : "Base de datos: sin conexión";
    }
}
