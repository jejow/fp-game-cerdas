using UnityEngine;

/// <summary>Default local match settings, retained on the menu prefab.</summary>
public class OfflineMatchSettings : MonoBehaviour
{
    [Range(0, 4)] public int allyBots = 0;
    [Range(1, 5)] public int enemyBots = 1;
    public BotDifficulty difficulty = BotDifficulty.Normal;
    public TeamSideType playerTeam = TeamSideType.Corpo;
    public OfflineMatchOptions Options => new OfflineMatchOptions(allyBots, enemyBots, difficulty, playerTeam).Normalized();
}
