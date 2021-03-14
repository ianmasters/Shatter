using System.Collections;
using UnityEngine;

namespace Shatter
{
#if !POO
    public class Shrapnel : PooledObject<Shrapnel>
    {
        private static readonly int MaterialOpacity = Shader.PropertyToID("_Opacity");
        private Coroutine fadeOutCoroutine;

        [MinMaxSlider(0f, 10f)] public MinMax fadeTime = new MinMax(0.1f, 1.0f);

        public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1f, 1, 0f);

        private void OnCollisionEnter(Collision other)
        {
            if(fadeOutCoroutine == null && other.collider.gameObject.isStatic)
                fadeOutCoroutine = StartCoroutine(FadeOut());
            // begin = true;
        }
        
        private IEnumerator FadeOut()
        {
            var materialPropertyBlock = new MaterialPropertyBlock();
            var diedTime = Time.time;
            
            var fade = fadeTime.RandomValue;
                
            var r = GetComponent<Renderer>();
            if (r)
            {
                // var materials = r.materials;
                // foreach (var material in materials)
                {
                    for (;;)
                    {
                        var t = (Time.time - diedTime) / fade;
                        if (t > 1) break;
                        // var opacity = 1 - t;
                        var opacity = fadeCurve.Evaluate(t);
                        r.GetPropertyBlock(materialPropertyBlock);
                        materialPropertyBlock.SetFloat(MaterialOpacity, opacity);
                        // material.SetFloat(MaterialOpacity, opacity);
                        // print($"{name} {material.name} opacity {opacity}");
                        r.SetPropertyBlock(materialPropertyBlock);
                        yield return null;
                    }
                }
            }
            RemoveFromScene();
        }

        public void SetPool(ObjectPool<Shrapnel> parentPool)
        {
            pool = parentPool;
        }
    }
#else
public class Shrapnel : MonoBehaviour
{
    public float fadeTime = 1;
    private bool begin;
    private Material[] materials;
    private static readonly int Opacity = Shader.PropertyToID("Opacity");

    private void Start()
    {
        fadeTime = Random.Range(0.3f, 1.0f);
        materials = GetComponent<Renderer>().sharedMaterials;
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
            else
            {
                foreach (var material in materials)
                {
                    material.SetFloat(Opacity, fadeTime);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.gameObject.isStatic)
            begin = true;
    }
}
#endif
}