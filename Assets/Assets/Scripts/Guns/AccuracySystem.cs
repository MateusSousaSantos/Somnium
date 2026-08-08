using UnityEngine;

// Stand-alone accuracy calculation shared by every gun (see GunBase.GetAccuracy()). Combines
// the player's concentration and how far the cursor currently is from the player into a base
// accuracy in [0,1], then applies the calling gun's own accuracyMultiplier on top - a gun with
// multiplier > 1 is more precise than the base formula suggests, < 1 is sloppier.
//
// All inputs are defensively clamped/guarded here rather than trusted from the caller, since
// PlayerStats.concentration is a bare mutable field that isn't itself guaranteed to stay in
// [0, 100] (state transitions add/subtract deltas of their own - see CrouchIdleState).
public static class AccuracySystem
{
    public const float MaxDistance = 10f;
    private const float DistanceWeightInfluence = 0.5f; // Reduces distance impact on accuracy (0-1, lower = less impact)

    public static float CalculateAccuracy(PlayerStats playerStats, float accuracyMultiplier)
    {
        if (playerStats == null) return 0f;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        float distanceFromPlayer = Vector3.Distance(mousePosition, playerStats.transform.position);

        // Normalize concentration to [0,1] - clamped defensively since concentration itself
        // isn't guaranteed to stay within [0, 100].
        float concentrationFactor = Mathf.Clamp01(playerStats.concentration / 100f);

        // 0 when aiming right on the player (no penalty), 1 at/beyond MaxDistance (full penalty).
        float distancePenalty = Mathf.Clamp01(distanceFromPlayer / MaxDistance) * DistanceWeightInfluence;

        float baseAccuracy = concentrationFactor * (1f - distancePenalty);

        // Multiplicative: >1 makes this gun more precise than the base formula, <1 sloppier.
        // Clamped so a bad multiplier (or an out-of-range concentration slipping through) can
        // never push accuracy outside [0,1] for callers like RevolverShoot's randomness calc.
        return Mathf.Clamp01(baseAccuracy * accuracyMultiplier);
    }
}
