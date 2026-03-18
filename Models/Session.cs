namespace DeepFocus.Models;

public class Session
{
    public string Date { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public List<Pause> Pauses { get; set; } = new();
    public string? End { get; set; }
}
