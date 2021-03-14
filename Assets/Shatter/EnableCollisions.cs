using System.Collections;
using UnityEngine;

namespace Shatter
{
    internal class EnableCollisions : MonoBehaviour
    {
        private Rigidbody rigidBody;
        
        private void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            if(rigidBody)
                StartCoroutine(WaitToStart());
        }
        
        private IEnumerator WaitToStart()
        {
            yield return new WaitForFixedUpdate();
            rigidBody.detectCollisions = true;
        }
    }
}
