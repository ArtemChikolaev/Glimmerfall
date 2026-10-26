using System;
using System.Globalization;
using System.Threading;

namespace Glimmerfall.Sim.Cli;

public static class Program
{
    private const string Usage = """
        glimmerfall-sim — headless-харнесс сима Glimmerfall.

        Команды:
          bench   [--scenario horde|run] [--ticks 3600] [--warmup 600] [--seed bench] [--enemies 5000] [--projectiles 2000]
          bot     [--policy kiter|afk|afk-after:SECONDS] [--runs 36] [--seed base] [--acts 1] [--act-seconds 480]
                  [--tier 0] [--squad lamplighter,warden,sparkwright] [--meta 0,0,0,0] [--set key=value]... [--jobs N]
                  [--max-minutes 30] [--dump out.json] [--verbose] [--trace N — посекундная трасса прогона N]
          career  [--runs 30] [--seed career] [--policy kiter] [--acts 3] [--act-seconds 480] [--set key=value]...
          record  --out FILE [--policy kiter] [--seed S] [--scenario run|horde] [--acts 1] [--act-seconds 120]
                  [--max-ticks N] [--every 600] [--set key=value]...
          verify  [--json] [--daily ГГГГ-ММ-ДД] FILE... (или - для stdin)
          digest  [--out FILE]   — набор прогонов для сверки дайджестов между ОС (CI)
        """;

    public static int Main(string[] argv)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        if (argv.Length == 0 || argv[0] is "-h" or "--help" or "help")
        {
            Console.WriteLine(Usage);
            return argv.Length == 0 ? 2 : 0;
        }
        var args = new Args(argv[1..]);
        try
        {
            return argv[0] switch
            {
                "bench" => BenchCommand.Run(args),
                "bot" => BotCommand.Run(args),
                "career" => CareerCommand.Run(args),
                "record" => ReplayCommands.Record(args),
                "verify" => ReplayCommands.Verify(args),
                "digest" => DigestCommand.Run(args),
                _ => Fail($"unknown command '{argv[0]}'\n\n{Usage}"),
            };
        }
        catch (ArgumentException e)
        {
            return Fail(e.Message);
        }
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 2;
    }
}
