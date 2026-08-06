using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Guarantees the persistent "Bootstrap" scene (Player, Main Camera, EventSystem) is loaded
/// before any gameplay scene's own Awake/Start code runs - even when a gameplay scene
/// (e.g. TestHub, OutsideHouse) is opened and played directly in the Editor instead of
/// starting from Bootstrap.unity.
///
/// Requires "Bootstrap" to be present in Build Settings (File > Build Settings).
/// </summary>
public static class SceneAutoBootstrap
{
    private const string BootstrapSceneName = "Bootstrap";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureBootstrapLoaded()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == BootstrapSceneName) return;

        if (SceneManager.GetSceneByName(BootstrapSceneName).isLoaded) return;

        SceneManager.LoadScene(BootstrapSceneName, LoadSceneMode.Additive);
    }
}
