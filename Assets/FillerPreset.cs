using UnityEngine;

namespace Assets
{
    public class FillerPreset
    {
        public FillerPreset(string resourceName, int count = 10, float localScale = 1, float distance = 1)
        {
            ResourceName = resourceName;
            Distance = distance;
            Count = count;
            LocalScale = new Vector3(localScale, localScale, localScale);
        }

        public string ResourceName { get; private set; }
        public int Count { get; private set; }
        public float Distance { get; private set; }
        public Vector3 LocalScale { get; private set; }
    }
}
