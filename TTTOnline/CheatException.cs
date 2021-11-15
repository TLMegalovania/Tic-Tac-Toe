namespace TTTOnline;
public class CheatException : Exception
{
    public CheatException() : base("The remote is cheating or there is a bug.") { }
}
