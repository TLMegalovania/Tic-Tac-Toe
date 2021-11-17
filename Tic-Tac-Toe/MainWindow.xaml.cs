using System.Net;
using System.Windows;

using TTTOnline;

namespace Tic_Tac_Toe;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CreateHost(object sender, RoutedEventArgs e)
    {
        GoBangServer server = new();
        CancellationTokenSource source = new();
        var clientTask = server.GetClientAsync(source.Token);
        /// Ugly.
        /// TODO: refactor
        var endpoint4 = (IPEndPoint)server.Listener4.LocalEndpoint;
        var endpoint6 = (IPEndPoint)server.Listener6.LocalEndpoint;
        var response = new ConnectWindow(clientTask, endpoint4.Port, endpoint6.Port, source).ShowDialog();
        if (response is null or false) return;
        new GameWindow(clientTask.Result).Show();
        Close();
    }

    private void ConnectHost(object sender, RoutedEventArgs e)
    {
        if (!IPAddress.TryParse(ipBox.Text, out var address))
        {
            MessageBox.Show("Invalid IP.");
            return;
        }
        if (!int.TryParse(portBox.Text, out int port))
        {
            MessageBox.Show("Invalid port.");
            return;
        }
        GoBangClient client = new();
        CancellationTokenSource source = new();
        var connectTask = client.ConnectAsync(address, port, source.Token);
        var response = new ConnectWindow(connectTask, source).ShowDialog();
        if (response is null or false) return;
        new GameWindow(client, client.ReceiveMoveAsync()).Show();
        Close();
    }
}
