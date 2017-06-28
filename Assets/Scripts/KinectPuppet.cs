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

        private readonly IDictionary<JointType, GameObject> _humanoidJoints;
        private readonly IDictionary<JointType, bool> _isConnected;

        private readonly IDictionary<JointType, GameObject> _joints;
        private readonly IDictionary<JointType, IDictionary<JointType, GameObject>> _bones;

        private BoneRelation[] _boneRelations;

        public KinectPuppet()
        {
            _joints = new Dictionary<JointType, GameObject>();
            _bones = new Dictionary<JointType, IDictionary<JointType, GameObject>>();
            _isConnected = new Dictionary<JointType, bool>();
            _humanoidJoints = new Dictionary<JointType, GameObject>();
        }

        public void Awake()
        {
            UpdateBoneRelations();
            ClearSkeletonSynchronization();
        }

        public void Start()
        {
            //Debug.Log("Start");
            AttachToKinect();
            CreateJoints();
            CreateBones();
            CreateHumanoidJoints();
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

        #region defaults

        private void ClearSkeletonSynchronization()
        {
            foreach (var jointType in JointTypes.All)
            {
                _isConnected[jointType] = false;
            }
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

        private void UpdateBoneRelations()
        {
            _boneRelations = new[]
            {
                //new Tuple(JointType.Head, JointType.Neck, Head,false),
                new BoneRelation(JointType.SpineShoulder, JointType.Neck, Neck, widthScale:2.1f),

                new BoneRelation(JointType.SpineMid, JointType.SpineShoulder, SpineMid, widthScale:2.1f),
                //new Tuple(JointType.SpineBase, JointType.SpineMid, SpineBase, false),

                new BoneRelation(JointType.ShoulderRight, JointType.ElbowRight, ShoulderRight),
                new BoneRelation(JointType.ElbowRight, JointType.WristRight, ElbowRight),
                new BoneRelation(JointType.WristRight, JointType.HandRight, WristRight),

                new BoneRelation(JointType.ShoulderLeft, JointType.ElbowLeft, ShoulderLeft),
                new BoneRelation(JointType.ElbowLeft, JointType.WristLeft, ElbowLeft),
                new BoneRelation(JointType.WristLeft, JointType.HandLeft, WristLeft),

                //new BoneRelation(JointType.SpineBase, JointType.HipLeft, SpineBase),
                //new Tuple(JointType.HipLeft, JointType.KneeLeft, HipLeft, false),

                new BoneRelation(JointType.KneeLeft, JointType.AnkleLeft, KneeLeft),
                new BoneRelation(JointType.AnkleLeft, JointType.FootLeft, FootLeft),

                //new Tuple(JointType.SpineBase, JointType.HipRight, SpineBase, false),
                //new Tuple(JointType.HipRight, JointType.KneeRight, HipLeft, false),

                new BoneRelation(JointType.KneeRight, JointType.AnkleRight, KneeRight),
                new BoneRelation(JointType.AnkleRight, JointType.FootRight, FootRight),
            };
        }


        #endregion

        #region Helpers

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

        private void CreateJoints()
        {
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                var jointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                jointObject.transform.parent = gameObject.transform;
                jointObject.name = string.Format("Joint_{0}", jointType);
                jointObject.transform.localScale += new Vector3(10, 10, 10);

                _joints[jointType] = jointObject;
            }
        }

        private void CreateBones()
        {
            foreach (var boneRelation in _boneRelations)
            {
                var boneObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                boneObject.transform.parent = gameObject.transform;
                boneObject.name = string.Format("Bone_{0}_{1}", boneRelation.JointFrom, boneRelation.JointTo);

                if (!_bones.ContainsKey(boneRelation.JointFrom))
                    _bones[boneRelation.JointFrom] = new Dictionary<JointType, GameObject>();

                _bones[boneRelation.JointFrom][boneRelation.JointTo] = boneObject;
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
            foreach (var boneRelation in _boneRelations)
            {
                DrawBone(boneRelation);
            }
        }

        private void DrawBone(BoneRelation boneRelation)
        {
            const float boneWidthScale = 10;

            var startJointType = boneRelation.JointFrom;
            var endJointType = boneRelation.JointTo;

            if (!_joints.ContainsKey(startJointType) || !_joints.ContainsKey(endJointType))
                return;

            if (!_bones.ContainsKey(startJointType) || !_bones[startJointType].ContainsKey(endJointType))
                return;

            var boneObject = _bones[startJointType][endJointType];

            var startJoint = _joints[startJointType];
            var endJoint = _joints[endJointType];

            if (startJoint == null || endJoint == null)
                return;

            var startJointPos = startJoint.transform.position;
            var endJointPos = endJoint.transform.position;

            var boneLength = Vector3.Distance(startJointPos, endJointPos);
            boneObject.transform.localScale = new Vector3(boneWidthScale * boneRelation.WidthScale, boneLength / 2, boneWidthScale * boneRelation.WidthScale);

            var boneVector = endJointPos - startJointPos;
            var rotationKinSys = Quaternion.FromToRotation(Vector3.up, boneVector);

            boneObject.transform.rotation = rotationKinSys;
            boneObject.transform.position = startJointPos + (endJointPos - startJointPos) / 2;
        }

        #endregion
    }
}
