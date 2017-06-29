using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets
{
    public class Respawner : MonoBehaviour
    {
        private GameObject _etalon;

        public void Start()
        {
            _etalon = Resources.Load("Ball") as GameObject;

            const float ballScale = 0.4f;
            const float ballDispersion = 5.4f;

            for (var i = 0; i < 1000; i++)
            {
                var newObject = Instantiate(_etalon, Vector3.zero, Quaternion.identity);

                var posX = ballDispersion * Random.value - ballDispersion / 2;
                var posZ = ballDispersion * Random.value - ballDispersion / 2;
                var posY = 2 + i / 10 * 2 * ballScale;

                newObject.transform.position = new Vector3(posX, posY, posZ);
                newObject.transform.localScale = new Vector3(ballScale, ballScale, ballScale);
            }
        }

        public void Update()
        {

        }
    }
}
