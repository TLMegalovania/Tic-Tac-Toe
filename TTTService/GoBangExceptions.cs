namespace TTTService;

public class NeverException : Exception
{
    public NeverException(string? message) : base($"Reached ideally unreachable code : {message}") { }
}
