using UnityEngine;

public class ObjectSound : MonoBehaviour {
    public float soundHeight;
    public AudioClip sound;

    public Vector2 soundPosition;

    private void Start()
    {
        if (sound != null)
        {
            // GameObject soundInstance = Instantiate(sound, soundPosition, Quaternion.identity);
            // soundInstance.transform.SetParent(transform);
        }
    }
}