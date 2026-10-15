using UnityEngine;

/// <summary>Local preferences and immutable input for a match.</summary>
public readonly struct OfflineMatchOptions
{
    public const int MaxAllyBots = 4;
    public const int MaxEnemyBots = 5;
    public readonly int AllyBots;
    public readonly int EnemyBots;
    public readonly BotDifficulty Difficulty;
    public readonly TeamSideType PlayerTeam;

    public OfflineMatchOptions(int allyBots, int enemyBots, BotDifficulty difficulty, TeamSideType playerTeam)
    {
        AllyBots = allyBots;
        EnemyBots = enemyBots;
        Difficulty = difficulty;
        PlayerTeam = playerTeam;
    }

    public OfflineMatchOptions Normalized() => new OfflineMatchOptions(
        Mathf.Clamp(AllyBots, 0, MaxAllyBots), Mathf.Clamp(EnemyBots, 1, MaxEnemyBots),
        Difficulty >= BotDifficulty.Easy && Difficulty <= BotDifficulty.Hard ? Difficulty : BotDifficulty.Normal,
        PlayerTeam == TeamSideType.Natif ? TeamSideType.Natif : TeamSideType.Corpo);

    public static OfflineMatchOptions Load()
    {
        var defaults = Object.FindFirstObjectByType<OfflineMatchSettings>();
        var options = defaults != null ? defaults.Options : new OfflineMatchOptions(0, 1, BotDifficulty.Normal, TeamSideType.Corpo);
        return new OfflineMatchOptions(
            PlayerPrefs.GetInt("Offline.AllyBots", options.AllyBots),
            PlayerPrefs.GetInt("Offline.EnemyBots", options.EnemyBots),
            (BotDifficulty)PlayerPrefs.GetInt("Offline.Difficulty", (int)options.Difficulty),
            (TeamSideType)PlayerPrefs.GetInt("Offline.PlayerTeam", (int)options.PlayerTeam)).Normalized();
    }

    public void Save()
    {
        var options = Normalized();
        PlayerPrefs.SetInt("Offline.AllyBots", options.AllyBots);
        PlayerPrefs.SetInt("Offline.EnemyBots", options.EnemyBots);
        PlayerPrefs.SetInt("Offline.Difficulty", (int)options.Difficulty);
        PlayerPrefs.SetInt("Offline.PlayerTeam", (int)options.PlayerTeam);
        PlayerPrefs.Save();
    }
}
