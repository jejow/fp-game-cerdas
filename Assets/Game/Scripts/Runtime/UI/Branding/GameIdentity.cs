using UnityEngine.UIElements;

/// <summary>The displayed identity shared by the opening, menus, and build tools.</summary>
public static class GameIdentity
{
    public const string Name = "STEEL PROTOCOL";
    public const string Tagline = "TACTICAL ROBOT COMBAT";
    public const string ExecutableName = "SteelProtocol.exe";

    public static void ApplyTo(VisualElement root)
    {
        root.Query<Label>(className: "game-title").ForEach(label => label.text = Name);
        root.Query<Label>(className: "game-tagline").ForEach(label => label.text = Tagline);
    }
}
