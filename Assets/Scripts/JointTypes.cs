using System;
using System.Linq;
using Windows.Kinect;

namespace Assets.Scripts
{
    public static class JointTypes
    {
        public static readonly JointType[] All = Enum.GetValues(typeof(JointType)).Cast<JointType>().ToArray();
    }
}
