using DeepFocus.Models;

namespace DeepFocus;

public class SessionManager
{
    private readonly ILogRepository _repository;

    public SessionManager(ILogRepository repository)
    {
        _repository = repository;
    }

    public (bool Success, string Message) StartSession(DateTime now)
    {
        var sessions = _repository.LoadSessions();
        if (FindActiveSession(sessions) is not null)
        {
            return (false, "An active session already exists.");
        }

        sessions.Add(new Session
        {
            Date = DateOnly.FromDateTime(now),
            Start = TimeOnly.FromDateTime(now),
            End = null
        });

        _repository.SaveSessions(sessions);
        return (true, $"Session started at {TimeOnly.FromDateTime(now):HH:mm:ss}.");
    }

    public (bool Success, string Message) EndSession(DateTime now)
    {
        var sessions = _repository.LoadSessions();
        var activeSession = FindActiveSession(sessions);
        if (activeSession is null)
        {
            return (false, "No active session to end.");
        }

        activeSession.End = TimeOnly.FromDateTime(now);
        _repository.SaveSessions(sessions);

        return (true, $"Session ended at {activeSession.End:HH:mm:ss}.");
    }

    public Session? GetActiveSession()
    {
        var sessions = _repository.LoadSessions();
        return FindActiveSession(sessions);
    }

    public List<Session> GetSessions(bool todayOnly, DateTime now)
    {
        var sessions = _repository.LoadSessions()
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Start)
            .ToList();

        if (!todayOnly)
        {
            return sessions;
        }

        var today = DateOnly.FromDateTime(now);
        return sessions.Where(s => s.Date == today).ToList();
    }

    private static Session? FindActiveSession(List<Session> sessions)
    {
        return sessions
            .Where(s => s.End is null)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Start)
            .LastOrDefault();
    }
}
