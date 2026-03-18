using DeepFocus;
using DeepFocus.Models;

namespace DeepFocus.Tests;

public class SessionManagerTests
{
    [Fact]
    public void StartSession_ShouldCreateActiveSession()
    {
        var repo = new InMemoryLogRepository();
        var manager = new SessionManager(repo);
        var now = new DateTime(2026, 3, 18, 9, 15, 0);

        var result = manager.StartSession(now);

        Assert.True(result.Success);
        Assert.Single(repo.Sessions);
        Assert.Equal(DateOnly.FromDateTime(now), repo.Sessions[0].Date);
        Assert.Equal(TimeOnly.FromDateTime(now), repo.Sessions[0].Start);
        Assert.Null(repo.Sessions[0].End);
    }

    [Fact]
    public void StartSession_WhenActiveExists_ShouldFail()
    {
        var repo = new InMemoryLogRepository
        {
            Sessions =
            {
                new Session
                {
                    Date = new DateOnly(2026, 3, 18),
                    Start = new TimeOnly(9, 0, 0),
                    End = null
                }
            }
        };
        var manager = new SessionManager(repo);

        var result = manager.StartSession(new DateTime(2026, 3, 18, 10, 0, 0));

        Assert.False(result.Success);
        Assert.Equal("An active session already exists.", result.Message);
    }

    [Fact]
    public void EndSession_WhenActiveExists_ShouldSetEnd()
    {
        var repo = new InMemoryLogRepository
        {
            Sessions =
            {
                new Session
                {
                    Date = new DateOnly(2026, 3, 18),
                    Start = new TimeOnly(9, 0, 0),
                    End = null
                }
            }
        };
        var manager = new SessionManager(repo);
        var now = new DateTime(2026, 3, 18, 11, 30, 0);

        var result = manager.EndSession(now);

        Assert.True(result.Success);
        Assert.Equal(new TimeOnly(11, 30, 0), repo.Sessions[0].End);
    }

    [Fact]
    public void EndSession_WhenNoActive_ShouldFail()
    {
        var repo = new InMemoryLogRepository();
        var manager = new SessionManager(repo);

        var result = manager.EndSession(new DateTime(2026, 3, 18, 11, 30, 0));

        Assert.False(result.Success);
        Assert.Equal("No active session to end.", result.Message);
    }

    [Fact]
    public void GetSessions_TodayOnly_ShouldReturnOnlyToday()
    {
        var repo = new InMemoryLogRepository
        {
            Sessions =
            {
                new Session
                {
                    Date = new DateOnly(2026, 3, 17),
                    Start = new TimeOnly(9, 0, 0),
                    End = new TimeOnly(10, 0, 0)
                },
                new Session
                {
                    Date = new DateOnly(2026, 3, 18),
                    Start = new TimeOnly(12, 0, 0),
                    End = new TimeOnly(13, 0, 0)
                }
            }
        };
        var manager = new SessionManager(repo);

        var sessions = manager.GetSessions(todayOnly: true, new DateTime(2026, 3, 18, 15, 0, 0));

        Assert.Single(sessions);
        Assert.Equal(new DateOnly(2026, 3, 18), sessions[0].Date);
    }

    private class InMemoryLogRepository : ILogRepository
    {
        public List<Session> Sessions { get; set; } = new();

        public List<Session> LoadSessions() => Sessions;

        public void SaveSessions(List<Session> sessions)
        {
            Sessions = sessions;
        }
    }
}
