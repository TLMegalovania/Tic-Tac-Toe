using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TTTOnline;

using TTTService;

namespace Tic_Tac_Toe;
/// <summary>
/// GameWindow.xaml 的交互逻辑
/// </summary>
public partial class GameWindow : Window
{
    private readonly GoBangClient _client;
    private readonly ImageBrush _blackBrush, _whiteBrush;
    private readonly BitmapImage _blackImage, _whiteImage;
    private GoBangTurnType thisTurn;
    private readonly GoBangTurnType yourTurn;
    private readonly (Button,bool)[,] _board;

    public GameWindow(GoBangClient client, bool isClient = false)
    {
        InitializeComponent();
        _client = client;
        _blackImage = new(new("./Images/black.gif", UriKind.Relative));
        _whiteImage = new(new("./Images/white.gif", UriKind.Relative));
        _blackBrush = new(_blackImage);
        _whiteBrush = new(_whiteImage);
        thisTurn = GoBangTurnType.Black;
        yourTurn = isClient ? GoBangTurnType.White : GoBangTurnType.Black;
        _board = new (Button,bool)[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Button btn = new();
                btn.Click += ClickBoard;
                _board[i, j] = (btn,true);
                Grid.SetRow(btn, i);
                Grid.SetColumn(btn, j);
                boardGrid.Children.Add(btn);
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
        if (sender is not UIElement ele) throw new NeverException("Not clicking UIElement.");
        if (thisTurn != yourTurn) return;
        int row = Grid.GetRow(ele), col = Grid.GetColumn(ele);
        if (!_board[row, col].Item2) return;
        ShowMove(row, col);
        _client.MoveAsync(row, col).ContinueWith(ContinueTask);
    }

    private void ShowMove(int x, int y)
    {
        _board[x, y].Item2 = false;
        switch (thisTurn)
        {
            case GoBangTurnType.Black:
                _board[x, y].Item1.Background = _blackBrush;
                thisTurn = GoBangTurnType.White;
                thisImage.Source = _whiteImage;
                break;
            case GoBangTurnType.White:
                _board[x, y].Item1.Background = _whiteBrush;
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
        else ShowMove(move.X, move.Y);
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
            var res = t.Result;
            if (res == null) return;
            ShowMove(res);
        });
    }
}
