namespace DeepFocus.Models;

public class Session
{
    public DateOnly Date { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly? End { get; set; }
}
