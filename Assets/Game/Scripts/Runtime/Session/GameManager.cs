using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake() => Cursor.visible = true;
    public Task Play() => Play(OfflineMatchOptions.Load());
    public Task Play(OfflineMatchOptions options) => OfflineMatch.StartAsync(options);

    private void OnApplicationQuit()
    {
        OfflineMatch.CancelLoading();
        PlayerHelpers.ClearTeams();
    }
}
