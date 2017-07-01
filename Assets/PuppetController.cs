using UnityEngine;

namespace Assets
{
    public class PuppetController : MonoBehaviour
    {
        private Rigidbody body;

        // Use this for initialization
        void Start()
        {
            body = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (body && Input.GetKeyDown("q"))
            {
                body.MovePosition(new Vector3(0, 20, -3));
                body.MoveRotation(Quaternion.Euler(90, 0, 0));
            }
        }
    }
}
