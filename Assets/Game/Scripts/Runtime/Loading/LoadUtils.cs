using System.Threading.Tasks;
using GameNetwork;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameNetwork.Utils
{
    public static class LoadUtils
    {
        public static async Task LoadSceneAsync(string sceneName, SessionData.LoadingSteps step)
        {
            if (SceneManager.GetActiveScene().name == sceneName) return;
            SessionData.Instance.UpdateLoading(step);
            using (new OfflineAudioTransition())
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (!operation.isDone)
                {
                    SessionData.Instance.UpdateLoading(step, operation.progress);
                    await Awaitable.NextFrameAsync();
                }
                // Let the new scene finish Start/initial rendering before resuming audio.
                await Awaitable.NextFrameAsync();
            }
        }

        public static async void ReturnToMainMenuAsync()
        {
            using var audioTransition = new OfflineAudioTransition();
            await QuitAsync();
            await LoadSceneAsync("MainMenu", SessionData.LoadingSteps.BackToMainMenu);
        }
        public static Task QuitAsync() => OfflineMatch.StopAsync();
    }
}



