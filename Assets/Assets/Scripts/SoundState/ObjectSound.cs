using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ObjectSound : MonoBehaviour {
    public int soundradius;

    private void Start()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
        GetComponent<CircleCollider2D>().radius = soundradius;
    }
}