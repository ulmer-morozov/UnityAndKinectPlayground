//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Windows.Kinect;
//using Assets.Scripts.Helpers;
//using Assets.Scripts.Helpers.Extensions;
//using UnityEngine;

//namespace Assets.Scripts
//{
//    public class KinectBodyServer : MonoBehaviour
//    {
//        public readonly IList<JointType> JointTypes;
//        public float AdditionalRotationY = (float)Math.PI / 2;


//        private KinectSensor _kinectSensor;
//        private BodyFrameReader _bodyFrameReader;
//        private Body[] _bodies;

//        public KinectBodyServer()
//        {
//            //выставляем дефолтные значения
//            JointTypes = Enum.GetValues(typeof(JointType)).Cast<JointType>().ToArray();
//        }

//        public void Start()
//        {
//            Debug.Log("Start");
//            InitializeKinect();
//        }

//        #region Helpers

//        private void InitializeKinect()
//        {
//            _kinectSensor = KinectSensor.GetDefault();

//            // get the depth (display) extents
//            var frameDescription = _kinectSensor.DepthFrameSource.FrameDescription;

//            // get size of joint space
//            var displayWidth = frameDescription.Width;
//            var displayHeight = frameDescription.Height;

//            // open the reader for the body frames
//            _bodyFrameReader = _kinectSensor.BodyFrameSource.OpenReader();
//            _bodyFrameReader.FrameArrived += Kinect_FrameArrived;

//            // set IsAvailableChanged event notifier
//            //this.kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

//            // open the sensor
//            _kinectSensor.Open();
//            Debug.Log(string.Format("Kinect Started {0} x {1}", displayWidth, displayHeight));
//        }

//        private void Kinect_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
//        {
//            using (var bodyFrame = e.FrameReference.AcquireFrame())
//            {
//                if (bodyFrame == null)
//                    return;

//                if (_bodies == null)
//                    _bodies = new Body[bodyFrame.BodyCount];

//                bodyFrame.GetAndRefreshBodyData(_bodies);

//                var trackedBodies = _bodies.Where(x => x.IsTracked).ToArray();
//                if (!trackedBodies.Any())
//                    return;

//                var body = trackedBodies.First();

//                DrawJoints(body);
//                DrawBones(body);

//                RotateHumanoid(body);
//            }
//        }

//        private void RotateHumanoid(Body body)
//        {
//            if (Humanoid == null)
//            {
//                Debug.Log("Humanoid not set!");
//                return;
//            }

//            foreach (var kvPair in _humanoidJoints)
//            {
//                var jointType = kvPair.Key;
//                var jointObject = kvPair.Value;

//                if (jointObject == null)
//                    continue;

//                //синхронизация части тела выключена
//                if (!_isConnected[jointType])
//                    continue;

//                var rotationKinSys = body.JointOrientations[jointType].Orientation.ToUnity();

//                var rotationUnitySys = new Quaternion
//                (
//                    rotationKinSys.x,
//                    -rotationKinSys.y,
//                    -rotationKinSys.z,
//                    rotationKinSys.w
//                );

//                var humanoidRotation = Humanoid.transform.rotation;
//                //var inversedHumanoidRotation = Quaternion.Inverse(humanoidRotation);

//                Quaternion addRot;

//                switch (jointType)
//                {
//                    case JointType.WristLeft:
//                        addRot = Quaternion.Euler(0, 90, 0);
//                        break;

//                    case JointType.WristRight:
//                        addRot = Quaternion.Euler(0, -90, 0);
//                        break;

//                    default:
//                        addRot = Quaternion.Euler(0, 0, 0);
//                        break;
//                }

//                jointObject.transform.rotation = humanoidRotation * rotationUnitySys * addRot;

//            }
//        }

//        #endregion
//    }
//}
