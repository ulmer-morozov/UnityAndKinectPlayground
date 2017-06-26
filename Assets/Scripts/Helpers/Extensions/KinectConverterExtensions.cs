using Windows.Kinect;
using UnityEngine;

namespace Assets.Scripts.Helpers.Extensions
{
    public static class KinectConverterExtensions
    {
        public static Quaternion ToUnity(this Windows.Kinect.Vector4 kinectQuaternion)
        {
            var quaterion = new Quaternion(kinectQuaternion.X, kinectQuaternion.Y, kinectQuaternion.Z, kinectQuaternion.W);
            return quaterion;
        }

        public static Vector3 ToUnity(this CameraSpacePoint kinectPoint)
        {
            var unityPoint = new Vector3(kinectPoint.X, kinectPoint.Y, kinectPoint.Z);
            return unityPoint;
        }
    }
}