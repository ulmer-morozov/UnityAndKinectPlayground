using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.VR.WSA.Persistence;

namespace Assets.Scripts
{
    public class KinectBodyRotationData
    {
        public readonly IDictionary<JointType, Quaternion> JointOrientations;

        public KinectBodyRotationData()
        {
            JointOrientations = new Dictionary<JointType, Quaternion>();
        }

        public void Set(JointType jointType, Quaternion quaternion)
        {
            JointOrientations[jointType] = quaternion;
        }

        public void Clear()
        {
            JointOrientations.Clear();
        }

        public bool IsEmpty
        {
            get { return !JointOrientations.Keys.Any(); } 
        }
    }
}
