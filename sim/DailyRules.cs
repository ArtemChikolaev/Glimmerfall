using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Determinism;

namespace Glimmerfall.Sim;

/// <summary>
/// Дейлик (PRD §10): общий сид дня по UTC, фиксированный отряд, базовый тир, нулевая мета, без оверрайдов.
/// Единственный источник правил: клиент строит конфиг отсюда, валидатор сверяет с ним отпечаток конфига реплея.
/// </summary>
public static class DailyRules
{
    private static readonly int[][] SquadOrders =
    {
        new[] { HeroIds.Lamplighter, HeroIds.Warden, HeroIds.Sparkwright },
        new[] { HeroIds.Lamplighter, HeroIds.Sparkwright, HeroIds.Warden },
        new[] { HeroIds.Warden, HeroIds.Lamplighter, HeroIds.Sparkwright },
        new[] { HeroIds.Warden, HeroIds.Sparkwright, HeroIds.Lamplighter },
        new[] { HeroIds.Sparkwright, HeroIds.Lamplighter, HeroIds.Warden },
        new[] { HeroIds.Sparkwright, HeroIds.Warden, HeroIds.Lamplighter },
    };

    /// <summary>Текст сида дня; date — «ГГГГ-ММ-ДД» по UTC.</summary>
    public static string SeedText(string date) => "daily-" + date;

    public static ulong Seed(string date) => SeedHash.FromString(SeedText(date));

    /// <summary>Канонический конфиг дейлика: полный забег, лидер и порядок отряда — по сиду дня.</summary>
    public static SimConfig Config(string date)
    {
        SimConfig config = SimConfig.Run();
        config.Squad = (int[])SquadOrders[Seed(date) % (ulong)SquadOrders.Length].Clone();
        return config;
    }
}
