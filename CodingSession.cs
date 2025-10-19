namespace CodingTracker;

public class CodingSession
{
    public int SessionId { get; init; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan Duration { get; set; }
}