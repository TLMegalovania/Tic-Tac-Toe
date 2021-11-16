using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    private readonly BitmapImage _blackImage, _whiteImage;
    private GoBangTurnType thisTurn;
    private readonly GoBangTurnType yourTurn;
    private readonly Image[,] _board;

    public GameWindow(GoBangClient client)
    {
        InitializeComponent();
        _client = client;
        _blackImage = new(new("./Images/black.gif", UriKind.Relative));
        _whiteImage = new(new("./Images/white.gif", UriKind.Relative));
        thisTurn = yourTurn = GoBangTurnType.Black;
        _board = new Image[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Image img = new();
                img.MouseLeftButtonDown += ClickBoard;
                _board[i, j] = img;
                Grid.SetRow(img, i);
                Grid.SetColumn(img, j);
                boardGrid.Children.Add(img);
            }
        }
        thisImage.Source = _blackImage;
        yourImage.Source = _blackImage;
    }

    public GameWindow(GoBangClient client, Task<MoveResult> moveTask) : this(client)
    {
        yourTurn = GoBangTurnType.White;
        yourImage.Source = _whiteImage;
        moveTask.ContinueWith(ContinueTask);
    }

    private void ClickBoard(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement ele) throw new NeverException("Not clicking UIElement.");
        int row = Grid.GetRow(ele), col = Grid.GetColumn(ele);
        ShowMove(row, col);
        _client.MoveAsync(row, col).ContinueWith(ContinueTask);
    }

    private void ShowMove(int x, int y)
    {
        if (_board[x, y].Source != null)
            switch (thisTurn)
            {
                case GoBangTurnType.Black:
                    _board[x, y].Source = _blackImage;
                    thisTurn = GoBangTurnType.White;
                    thisImage.Source = _blackImage;
                    break;
                case GoBangTurnType.White:
                    _board[x, y].Source = _whiteImage;
                    thisTurn = GoBangTurnType.Black;
                    thisImage.Source = _whiteImage;
                    break;
                default:
                    throw new NeverException("Invalid this turn.");
            }

    }

    private void ShowMove(MoveResult move)
    {
        ShowMove(move.X, move.Y);
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
    }

    private void ContinueTask(Task<MoveResult> t)
    {
        Dispatcher.Invoke(() =>
        {
            if (t.IsFaulted)
            {
                MessageBox.Show(t.Exception!.GetBaseException().Message);
                Close();
                return;
            }
            ShowMove(t.Result);
        });
    }
}
