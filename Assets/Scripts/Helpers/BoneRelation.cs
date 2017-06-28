using Windows.Kinect;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class BoneRelation
    {
        public JointType JointFrom { get; private set; }
        public JointType JointTo { get; private set; }
        public GameObject BodyPart { get; private set; }

        public BoneRelation(JointType jointFrom, JointType jointTo, GameObject bodyPart)
        {
            JointFrom = jointFrom;
            JointTo = jointTo;
            BodyPart = bodyPart;
        }
    }
}