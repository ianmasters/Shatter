using System.Collections;
using UnityEngine;

namespace Shatter
{
    internal class EnableCollisions : MonoBehaviour
    {
        // public uint enableAfterFrames = 1;
        public bool enableGravity; 

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
            
            // for(; enableAfterFrames > 0; --enableAfterFrames)
            //     yield return null;
            
            if (enableGravity) rigidBody.useGravity = true;
            rigidBody.detectCollisions = true;
        }
    }
}
