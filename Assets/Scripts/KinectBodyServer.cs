using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using Assets.Scripts.Helpers.Extensions;
using UnityEngine;

namespace Assets.Scripts
{
    public class KinectBodyServer : MonoBehaviour
    {
        private static KinectData _kinectData;
        private static IList<Body> _bodies;

        private static KinectSensor _kinectSensor;
        private static BodyFrameReader _bodyFrameReader;
        private static IList<IKinectRotationDataReciever> _recievers;

        public void Awake()
        {
            Debug.Log("Awake KinectBodyServer");
            InitializeData();
        }

        public void Start()
        {
            Debug.Log("Start KinectBodyServer");
            InitializeKinect();
        }

        public void RegisterReciever(IKinectRotationDataReciever reciever)
        {
            if (_recievers.Contains(reciever))
                return;

            _recievers.Add(reciever);
        }

        #region Helpers

        private static void InitializeData()
        {
            if (_kinectData == null)
            {
                _kinectData = new KinectData();
            }

            if (_recievers == null)
            {
                _recievers = new List<IKinectRotationDataReciever>();
            }
        }

        private static void InitializeKinect()
        {
            //if (_kinectSensor != null)
            //    return;

            Debug.Log("InitializeKinect");

            _kinectSensor = KinectSensor.GetDefault();

            // get the depth (display) extents
            var frameDescription = _kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            var displayWidth = frameDescription.Width;
            var displayHeight = frameDescription.Height;

            // open the reader for the body frames
            _bodyFrameReader = _kinectSensor.BodyFrameSource.OpenReader();
            _bodyFrameReader.FrameArrived += Kinect_FrameArrived;

            // set IsAvailableChanged event notifier
            //this.kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            // open the sensor
            _kinectSensor.Open();
            Debug.Log(string.Format("Kinect Started {0} x {1}", displayWidth, displayHeight));
        }

        private static void Kinect_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            //Debug.Log(string.Format("Kinect_FrameArrived e {0}", e));
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    //Debug.Log("bodyFrame == null");
                    return;
                }

                if (_bodies == null)
                    _bodies = new Body[bodyFrame.BodyCount];

                if (_kinectData.RotationDataArray == null)
                    _kinectData.RotationDataArray = new KinectBodyRotationData[bodyFrame.BodyCount];

                bodyFrame.GetAndRefreshBodyData(_bodies);
                _bodies = _bodies.OrderByDescending(x => x.IsTracked).ToArray();

                var anyTrackedBody = false;

                for (var i = 0; i < _bodies.Count; i++)
                {
                    var body = _bodies[i];

                    if (_kinectData.RotationDataArray[i] == null)
                        _kinectData.RotationDataArray[i] = new KinectBodyRotationData();

                    var rotationData = _kinectData.RotationDataArray[i];

                    if (body == null)
                    {
                        //Debug.Log(string.Format("body {0} is NULL", i));
                        continue;
                    }

                    //Debug.Log(string.Format("body {0} IsTracked = {1}", i, body.IsTracked));
                    if (!body.IsTracked)
                    {
                        rotationData.Clear();
                        continue;
                    }
                    anyTrackedBody = true;
                    SetRotationData(body, rotationData);
                }

                if (!anyTrackedBody)
                    return;

                SendDataToRecievers();
            }
        }

        private static void SendDataToRecievers()
        {
            foreach (var reciever in _recievers)
            {
                reciever.UpdatePuppet(_kinectData);
            }
        }

        private static void SetRotationData(Body body, KinectBodyRotationData data)
        {
            foreach (var jointType in JointTypes.All)
            {
                var rotationKinSys = body.JointOrientations[jointType].Orientation.ToUnity();

                var rotationUnitySys = new Quaternion
                (
                    rotationKinSys.x,
                    -rotationKinSys.y,
                    -rotationKinSys.z,
                    rotationKinSys.w
                );

                Quaternion addRot;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (jointType)
                {
                    case JointType.WristLeft:
                        addRot = Quaternion.Euler(0, 90, 0);
                        break;

                    case JointType.WristRight:
                        addRot = Quaternion.Euler(0, -90, 0);
                        break;

                    default:
                        addRot = Quaternion.Euler(0, 0, 0);
                        break;
                }

                var resultRotation = rotationUnitySys * addRot;
                data.Set(jointType, resultRotation);
            }
        }

        #endregion
    }
}
