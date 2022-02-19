using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using TTTOnline;

using TTTService;

namespace Tic_Tac_Toe;
/// <summary>
/// GameWindow.xaml 的交互逻辑
/// </summary>
public partial class GameWindow : Window
{
    private readonly GoBangClient _client;
    private readonly ImageSource _blackImage, _whiteImage;
    private GoBangTurnType thisTurn;
    private readonly GoBangTurnType yourTurn;
    // Rectangle if null, else Image.
    private readonly FrameworkElement[,] _board;

    public GameWindow(GoBangClient client, bool isClient = false)
    {
        InitializeComponent();
        _client = client;
        // Forced convention.
        _blackImage = (ImageSource)Resources["Black"];
        _whiteImage = (ImageSource)Resources["White"];
        thisTurn = GoBangTurnType.Black;
        yourTurn = isClient ? GoBangTurnType.White : GoBangTurnType.Black;
        _board = new FrameworkElement[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Rectangle idle = new()
                {
                    Fill = Brushes.AliceBlue
                };
                idle.MouseLeftButtonDown += ClickBoard;
                _board[i, j] = idle;
                boardGrid.Children.Add(idle);
                Grid.SetRow(idle, i);
                Grid.SetColumn(idle, j);
            }
        }
        thisImage.Source = _blackImage;
        yourImage.Source = isClient ? _whiteImage : _blackImage;
    }

    public GameWindow(GoBangClient client, Task<MoveResult?> moveTask) : this(client, true)
    {
        moveTask.ContinueWith(ContinueTask);
    }

    private void ClickBoard(object sender, RoutedEventArgs e)
    {
        if (sender is not UIElement ele)
        {
            throw new NeverException("Not clicking UIElement.");
        }

        if (thisTurn != yourTurn)
        {
            return;
        }

        if (ele is not Rectangle)
        {
            return;
        }

        int row = Grid.GetRow(ele), col = Grid.GetColumn(ele);
        ShowMove(row, col);
        _client.MoveAsync(row, col).ContinueWith(ContinueTask);

    }

    private void ShowMove(int x, int y)
    {
        Image thei;
        FrameworkElement? ele = _board[x, y];
        switch (ele)
        {
            case Image i:
                thei = i;
                break;
            default:
                boardGrid.Children.Remove(ele);
                thei = new();
                Grid.SetRow(thei, x);
                Grid.SetColumn(thei, y);
                boardGrid.Children.Add(thei);
                break;
        }
        switch (thisTurn)
        {
            case GoBangTurnType.Black:
                thei.Source = _blackImage;
                thisTurn = GoBangTurnType.White;
                thisImage.Source = _whiteImage;
                break;
            case GoBangTurnType.White:
                thei.Source = _whiteImage;
                thisTurn = GoBangTurnType.Black;
                thisImage.Source = _blackImage;
                break;
            default:
                throw new NeverException("Invalid this turn.");
        }
    }

    private void ShowMove(MoveResult move)
    {
        if (move.Winner != GoBangTurnType.Null)
        {
            if (move.Winner == GoBangTurnType.Tie)
            {
                MessageBox.Show("Game ends in a tie.");
            }
            else if (move.Winner == yourTurn)
            {
                MessageBox.Show("You wins!");
            }
            else
            {
                MessageBox.Show("You lose.");
            }
            Close();
        }
        else
        {
            ShowMove(move.X, move.Y);
        }
    }

    private void ContinueTask(Task<MoveResult?> t)
    {
        Dispatcher.Invoke(() =>
        {
            if (t.IsFaulted)
            {
                MessageBox.Show(t.Exception!.GetBaseException().Message);
                Close();
                return;
            }
            MoveResult? res = t.Result;
            if (res == null)
            {
                return;
            }

            ShowMove(res);
        });
    }
}
