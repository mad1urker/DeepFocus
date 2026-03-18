namespace DeepFocus;

internal static class Program
{
    private static int Main(string[] args)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "deepfocus_log.json");
        var repository = new LogRepository(logPath);
        var manager = new SessionManager(repository);
        var renderer = new ConsoleRenderer();

        if (args.Length == 0 || args[0] == "--help")
        {
            renderer.RenderHelp();
            return 0;
        }

        var now = DateTime.Now;

        switch (args[0])
        {
            case "start":
            {
                var result = manager.StartSession(now);
                renderer.RenderMessage(result.Message);
                return result.Success ? 0 : 1;
            }
            case "pause-st":
            {
                var result = manager.StartPause(now);
                renderer.RenderMessage(result.Message);
                return result.Success ? 0 : 1;
            }
            case "pause-end":
            {
                var result = manager.EndPause(now);
                renderer.RenderMessage(result.Message);
                return result.Success ? 0 : 1;
            }
            case "end":
            {
                var result = manager.EndSession(now);
                if (!string.IsNullOrWhiteSpace(result.Warning))
                {
                    renderer.RenderMessage(result.Warning);
                }

                renderer.RenderMessage(result.Message);
                return result.Success ? 0 : 1;
            }
            case "focus-check":
            {
                var todayOnly = args.Length > 1 && args[1] == "--today";
                if (args.Length > 1 && !todayOnly)
                {
                    renderer.RenderMessage("Unknown option. Use 'df --help'.");
                    return 1;
                }

                var sessions = manager.GetSessions(todayOnly, now);
                renderer.RenderSessions(sessions, now);
                return 0;
            }
            default:
                renderer.RenderMessage("Unknown command. Use 'df --help'.");
                return 1;
        }
    }
}
