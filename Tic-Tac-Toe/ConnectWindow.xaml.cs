using System.Net;
using System.Windows;

using TTTOnline;

namespace Tic_Tac_Toe;
/// <summary>
/// ConnectWindow.xaml 的交互逻辑
/// </summary>
public partial class ConnectWindow : Window
{
    private readonly CancellationTokenSource _source;

    public ConnectWindow(Task<GoBangClient> clientTask, int port4, int port6, CancellationTokenSource source)
    {
        InitializeComponent();
        _source = source;
        ipList.ItemsSource = Dns.GetHostAddresses("");
        port4Label.Content = port4;
        port6Label.Content = port6;
        HandleConnection(clientTask);
    }

    public ConnectWindow(Task<Task<MoveResult>> connectTask, CancellationTokenSource source)
    {
        InitializeComponent();
        _source = source;
        hostCanvas.Visibility = Visibility.Collapsed;
        HandleConnection(connectTask);
    }

    private void HandleConnection(Task task)
    {
        task.ContinueWith(t =>
        {
            Dispatcher.Invoke(() =>
            {
                if (t.IsFaulted)
                {
                    MessageBox.Show(t.Exception!.GetBaseException().Message);
                }
                else if (t.IsCanceled) return;
                else DialogResult = true;
                Close();
            });
        });
    }

    private void CancelConnect(object sender, RoutedEventArgs e)
    {
        _source.Cancel();
    }
}
