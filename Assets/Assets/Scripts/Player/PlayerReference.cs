using UnityEngine;

/// <summary>
/// Marks the persistent Player root and exposes a single documented way for other systems
/// (camera follow, future enemies/HUD) to find "the current player" instead of
/// GameObject.FindWithTag or cross-scene Inspector references.
///
/// Lives on the Player prefab root, alongside PlayerStateController/PlayerStats - kept as its
/// own component rather than folded into either of those so the state machine files stay
/// untouched.
/// </summary>
public class PlayerReference : MonoBehaviour
{
    public static PlayerReference Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A duplicate Player already exists (e.g. Bootstrap got loaded twice) - keep the
            // original and remove this one instead of ending up with two active players.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
