using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts
{
    public class KinectPuppet : MonoBehaviour, IKinectRotationDataReciever
    {
        public GameObject Humanoid;

        public GameObject Head;
        public GameObject Neck;

        public GameObject ShoulderRight;
        public GameObject ShoulderBaseRight;

        public GameObject ElbowRight;
        public GameObject WristRight;

        public GameObject ShoulderLeft;
        public GameObject ShoulderBaseLeft;

        public GameObject ElbowLeft;
        public GameObject WristLeft;

        public GameObject SpineBase;
        public GameObject SpineMid;
        public GameObject SpineShoulder;

        public GameObject HipLeft;
        public GameObject KneeLeft;
        public GameObject AnkleLeft;

        public GameObject FootLeft;

        public GameObject HipRight;
        public GameObject KneeRight;

        public GameObject AnkleRight;
        public GameObject FootRight;

        private IDictionary<JointType, GameObject> _humanoidJoints;
        private IDictionary<JointType, bool> _isConnected;

        private IDictionary<JointType, GameObject> _joints;
        private IDictionary<JointType, GameObject> _bones;

        public void Awake()
        {
            _joints = new Dictionary<JointType, GameObject>();
            _bones = new Dictionary<JointType, GameObject>();
            _isConnected = new Dictionary<JointType, bool>();
            _humanoidJoints = new Dictionary<JointType, GameObject>();

            //инициализируем данные
            foreach (var jointType in JointTypes.All)
            {
                _isConnected[jointType] = false;
            }
        }

        public void Start()
        {
            //Debug.Log("Start");
            AttachToKinect();
            CreateJoints();
            CreateBones();
            CreateHumanoidJoints();
        }

        private void AttachToKinect()
        {
            Debug.Log("try attach to kinect server");
            var kinectSerer = GetComponent<KinectBodyServer>();
            if (kinectSerer == null)
                return;

            kinectSerer.RegisterReciever(this);
            Debug.Log("reciever attached");
        }

        private void BindHumanoidJoint(JointType jointType, GameObject bindedObject, bool connected = true)
        {
            _humanoidJoints[jointType] = bindedObject;
            _isConnected[jointType] = connected;
        }

        private void CreateHumanoidJoints()
        {
            BindHumanoidJoint(JointType.Head, Head, connected: false);
            BindHumanoidJoint(JointType.Neck, SpineShoulder);

            BindHumanoidJoint(JointType.SpineShoulder, SpineMid);
            BindHumanoidJoint(JointType.SpineMid, SpineBase);
            //BindHumanoidJoint(JointType.SpineBase, null);

            BindHumanoidJoint(JointType.ShoulderRight, ShoulderBaseRight);
            BindHumanoidJoint(JointType.ElbowRight, ShoulderRight);
            BindHumanoidJoint(JointType.WristRight, ElbowRight);
            BindHumanoidJoint(JointType.HandRight, WristRight);

            BindHumanoidJoint(JointType.ShoulderLeft, ShoulderBaseLeft);
            BindHumanoidJoint(JointType.ElbowLeft, ShoulderLeft);
            BindHumanoidJoint(JointType.WristLeft, ElbowLeft);
            BindHumanoidJoint(JointType.HandLeft, WristLeft);

            //BindHumanoidJoint(JointType.HipLeft, null);
            BindHumanoidJoint(JointType.KneeLeft, HipLeft);
            BindHumanoidJoint(JointType.AnkleLeft, KneeLeft);
            BindHumanoidJoint(JointType.FootLeft, AnkleLeft, connected: false);

            //BindHumanoidJoint(JointType.HipRight, null);
            BindHumanoidJoint(JointType.KneeRight, HipRight);
            BindHumanoidJoint(JointType.AnkleRight, KneeRight);
            BindHumanoidJoint(JointType.FootRight, AnkleRight, connected: false);
        }

        public void UpdatePuppet(KinectData data)
        {
            var rotationData = data.RotationDataArray.FirstOrDefault(x => !x.IsEmpty);
            if (rotationData == null)
                return;

            //Debug.Log("update puppet");
            RotateHumanoid(rotationData);
            DrawJoints();
            DrawBones();
        }

        #region Helpers

        private void CreateJoints()
        {
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                var jointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                jointObject.transform.parent = gameObject.transform;
                jointObject.name = string.Format("Joint_{0}", jointType);
                jointObject.transform.localScale += new Vector3(20, 20, 20);

                _joints[jointType] = jointObject;
            }
        }

        private void CreateBones()
        {
            foreach (var jointType in JointTypes.All)
            {
                var boneObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                boneObject.transform.parent = gameObject.transform;
                boneObject.name = string.Format("Bone_{0}", jointType);

                _bones[jointType] = boneObject;
            }
        }

        private void RotateHumanoid(KinectBodyRotationData rotationData)
        {
            if (Humanoid == null)
            {
                Debug.Log("Humanoid not set!");
                return;
            }

            foreach (var kvPair in _humanoidJoints)
            {
                var jointType = kvPair.Key;
                var jointObject = kvPair.Value;

                if (jointObject == null)
                    continue;

                //синхронизация части тела выключена
                if (!_isConnected[jointType])
                    continue;

                var rotationUnitySys = rotationData.JointOrientations[jointType];
                var humanoidRotation = Humanoid.transform.rotation;

                jointObject.transform.rotation = humanoidRotation * rotationUnitySys;
            }
        }

        private void DrawJoints()
        {
            //обрисовывем нашего человечка
            foreach (var jointType in JointTypes.All)
            {
                var jointObject = _joints[jointType];

                if (!_humanoidJoints.ContainsKey(jointType))
                {
                    jointObject.SetActive(false);
                    continue;
                }
                
                var humanoidPart = _humanoidJoints[jointType];
                if (humanoidPart == null)
                {
                    jointObject.SetActive(false);
                    continue;
                }

                jointObject.transform.position = humanoidPart.transform.position;
                jointObject.transform.rotation = humanoidPart.transform.rotation;
            }
        }

        private void DrawBones()
        {
            Tuple[] pairs =
            {
                //new Tuple(JointType.Head, JointType.Neck, Head,false),
                new Tuple(JointType.SpineShoulder, JointType.Neck, Neck, false),

                new Tuple(JointType.SpineMid, JointType.SpineShoulder, SpineMid, false),
                //new Tuple(JointType.SpineBase, JointType.SpineMid, SpineBase, false),

                new Tuple(JointType.ShoulderRight, JointType.ElbowRight, ShoulderRight, false),
                new Tuple(JointType.ElbowRight, JointType.WristRight, ElbowRight, false),
                new Tuple(JointType.WristRight, JointType.HandRight, WristRight, false),

                new Tuple(JointType.ShoulderLeft, JointType.ElbowLeft, ShoulderLeft, false),
                new Tuple(JointType.ElbowLeft, JointType.WristLeft, ElbowLeft, false),
                new Tuple(JointType.WristLeft, JointType.HandLeft, WristLeft, false),

                new Tuple(JointType.SpineBase, JointType.HipLeft, SpineBase, false),
                //new Tuple(JointType.HipLeft, JointType.KneeLeft, HipLeft, false),

                new Tuple(JointType.KneeLeft, JointType.AnkleLeft, KneeLeft, false),
                new Tuple(JointType.AnkleLeft, JointType.FootLeft, FootLeft, true),

                //new Tuple(JointType.SpineBase, JointType.HipRight, SpineBase, false),
                //new Tuple(JointType.HipRight, JointType.KneeRight, HipLeft, false),

                new Tuple(JointType.KneeRight, JointType.AnkleRight, KneeRight, false),
                new Tuple(JointType.AnkleRight, JointType.FootRight, FootRight, true),
            };

            foreach (var tuple in pairs)
            {
                DrawBone(tuple.First, tuple.Second);
            }
        }

        private void DrawBone(JointType startJointType, JointType endJointType)
        {
            const float boneWidthScale = 20;

            if (
                !_joints.ContainsKey(startJointType)
                ||
                !_joints.ContainsKey(endJointType)
                ||
                !_bones.ContainsKey(endJointType)
            )
            {
                return;
            }

            var boneObject = _bones[endJointType];

            var startJoint = _joints[startJointType];
            var endJoint = _joints[endJointType];

            if (startJoint == null || endJoint == null)
                return;

            var startJointPos = startJoint.transform.position;
            var endJointPos = endJoint.transform.position;

            var boneLength = Vector3.Distance(startJointPos, endJointPos);
            boneObject.transform.localScale = new Vector3(boneWidthScale, boneLength / 2, boneWidthScale);

            var boneVector = endJointPos - startJointPos;
            var rotationKinSys = Quaternion.FromToRotation(Vector3.up, boneVector);

            boneObject.transform.rotation = rotationKinSys;
            boneObject.transform.position = startJointPos + (endJointPos - startJointPos) / 2;
        }

        #endregion
    }
}
