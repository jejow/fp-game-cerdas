using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameNetwork
{
    public class SessionData : INotifyBindablePropertyChanged
    {
        public enum LoadingSteps
        {
            StartLoading, CreateWorld, LoadGameScene, LoadServer, LoadClient,
            WorldReplication, LoadingDone, UnloadingGame, BackToMainMenu
        }

        private static readonly Dictionary<LoadingSteps, (string text, float start, float end)> Steps = new()
        {
            { LoadingSteps.StartLoading, ("Preparing offline match...", 0f, 0f) },
            { LoadingSteps.CreateWorld, ("Preparing local gameplay...", 0.1f, 0.1f) },
            { LoadingSteps.LoadGameScene, ("Loading map...", 0.2f, 0.5f) },
            { LoadingSteps.LoadServer, ("Loading game rules...", 0.5f, 0.7f) },
            { LoadingSteps.LoadClient, ("Loading player view...", 0.7f, 0.8f) },
            { LoadingSteps.WorldReplication, ("Preparing characters...", 0.8f, 0.9f) },
            { LoadingSteps.LoadingDone, ("Starting offline match...", 1f, 1f) },
            { LoadingSteps.UnloadingGame, ("Leaving match...", 0f, 0f) },
            { LoadingSteps.BackToMainMenu, ("Opening main menu...", 0f, 1f) }
        };

        private static SessionData instance;
        public static SessionData Instance => instance ??= new SessionData();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => instance = new SessionData();

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        private void Notify([CallerMemberName] string property = "") =>
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        private LoadingSteps? lastLoggedStep;

        public void UpdateLoading(LoadingSteps step, float progress = 0f)
        {
            var current = Steps[step];
            LoadingProgress = Mathf.Lerp(current.start, current.end, progress);
            LoadingStatusText = current.text;
            if (lastLoggedStep == step) return;
            lastLoggedStep = step;
            Debug.Log(current.text);
        }

        private float loadingProgress;
        public const string LoadingProgressPropertyName = nameof(LoadingProgress);
        [CreateProperty]
        public float LoadingProgress
        {
            get => loadingProgress;
            set { loadingProgress = value; Notify(); }
        }
        private string loadingStatusText = "";
        public const string LoadingStatusTextPropertyName = nameof(LoadingStatusText);
        [CreateProperty]
        public string LoadingStatusText
        {
            get => loadingStatusText;
            set
            {
                if (loadingStatusText == value) return;
                loadingStatusText = value;
                Notify();
            }
        }
    }
}
