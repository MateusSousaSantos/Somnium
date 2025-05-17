using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CustomObjectSound : ObjectSound {
    public float stepSoundRadius;

    private void Start()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        soundradius = (int)stepSoundRadius;
    }
}