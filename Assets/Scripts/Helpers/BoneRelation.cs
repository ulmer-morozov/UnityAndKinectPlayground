using Windows.Kinect;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class BoneRelation
    {
        public JointType JointFrom { get; private set; }
        public JointType JointTo { get; private set; }
        public GameObject BodyPart { get; private set; }
        public float WidthScale { get; private set; }

        public BoneRelation(JointType jointFrom, JointType jointTo, GameObject bodyPart, float widthScale = 1)
        {
            JointFrom = jointFrom;
            JointTo = jointTo;
            BodyPart = bodyPart;
            WidthScale = widthScale;
        }
    }
}