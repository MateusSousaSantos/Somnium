using UnityEngine;

public class StepSound : ObjectSound {
    private void Awake()
    {
        soundHeight = 0.5f; // Adjust the height of the sound above the object
        soundPosition = transform.position;
    }
}