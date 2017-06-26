using Windows.Kinect;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public class Tuple
    {
        public JointType First { get; private set; }
        public JointType Second { get; private set; }
        public GameObject Third { get; private set; }
        public bool UseCustomRotation { get; private set; }

        public Tuple(JointType first, JointType second, GameObject third, bool useCustomRotation)
        {
            First = first;
            Second = second;
            Third = third;
            UseCustomRotation = useCustomRotation;
        }
    }
}