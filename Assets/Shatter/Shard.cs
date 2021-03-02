using UnityEngine;
using Random = UnityEngine.Random;

public class Shard : MonoBehaviour
{
    public float fadeTime = 1;
    private bool begin;

    private void Start()
    {
        fadeTime = Random.Range(0.3f, 1.0f);
    }

    private void Update()
    {
        if (begin)
        {
            fadeTime -= Time.deltaTime;
            if (fadeTime <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.gameObject.isStatic)
            begin = true;
    }
}
