using DeepFocus.Models;

namespace DeepFocus;

public class ConsoleRenderer
{
    public void RenderHelp()
    {
        Console.WriteLine("DeepFocus (df) - Focus session tracker");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  df start          Start a focus session");
        Console.WriteLine("  df end            End active session");
        Console.WriteLine("  df status         Show active session status");
        Console.WriteLine("  df logs           Show all logs");
        Console.WriteLine("  df logs --today   Show today's log only");
        Console.WriteLine("  df --help         Show help");
    }

    public void RenderMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void RenderStatus(Session? activeSession, DateTime now)
    {
        if (activeSession is null)
        {
            Console.WriteLine("No active session.");
            return;
        }

        Console.WriteLine($"Active session: {FormatSessionLine(activeSession, now)}");
    }

    public void RenderSessions(List<Session> sessions, DateTime now)
    {
        if (sessions.Count == 0)
        {
            Console.WriteLine("No sessions found.");
            return;
        }

        foreach (var session in sessions)
        {
            Console.WriteLine(FormatSessionLine(session, now));
        }
    }

    private static string FormatSessionLine(Session session, DateTime now)
    {
        var endText = session.End?.ToString("HH:mm:ss") ?? "in progress";
        var netFocus = CalculateNetFocus(session, now);
        return $"{session.Date:yyyy-MM-dd} | Start: {session.Start:HH:mm:ss} | End: {endText} | Net focus: {Math.Max(0, (int)netFocus.TotalHours)}h {netFocus.Minutes:D2}m";
    }

    private static TimeSpan CalculateNetFocus(Session session, DateTime now)
    {
        var start = session.Date.ToDateTime(session.Start);
        var end = session.End is null
            ? now
            : session.Date.ToDateTime(session.End.Value);

        if (end < start)
        {
            return TimeSpan.Zero;
        }

        return end - start;
    }
}
