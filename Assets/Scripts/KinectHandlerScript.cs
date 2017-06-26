using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using Assets.Scripts.Helpers;
using Assets.Scripts.Helpers.Extensions;
using UnityEngine;

namespace Assets.Scripts
{
    public class KinectHandlerScript : MonoBehaviour
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

        private GameObject[] _bodyParts;

        public float AdditionalRotationY = (float)Math.PI / 2;


        private KinectSensor _kinectSensor;
        private BodyFrameReader _bodyFrameReader;
        private Body[] _bodies;


        private IDictionary<JointType, GameObject> _humanoidJoints;


        private IDictionary<JointType, GameObject> _joints;
        private IDictionary<JointType, GameObject> _bones;

        private IDictionary<GameObject, float> _initialYRotations;

        public const int PosK = 100;

        public void Awake()
        {
            _joints = new Dictionary<JointType, GameObject>();
            _bones = new Dictionary<JointType, GameObject>();
            _initialYRotations = new Dictionary<GameObject, float>();

            CreateJoints();
            CreateBones();
            CreateHumanoidJoints();

            _bodyParts = new[]
            {
                //Head,
                //Neck,
                //Neck1,

                //RightArm,
                //RightForeArm,

                //LeftArm,
                //LeftForeArm,

                //Spine,
                //LowerBack,

                //LHipJoint,
                //LeftUpLeg,

                //LeftLeg,
                 FootLeft,

                //RHipJoint,
                //RightUpLeg,

                //RightLeg,
                //RightFoot
            }.Where(x => x != null).ToArray();

            foreach (var bodyPart in _bodyParts)
            {
                var rotation = bodyPart.transform.localRotation.eulerAngles.y;
                //_initialYRotations[bodyPart] = rotation;

                Debug.Log(bodyPart + " init rot: " + rotation);
            }
        }

        private void CreateHumanoidJoints()
        {
            _humanoidJoints = new Dictionary<JointType, GameObject>();

            _humanoidJoints[JointType.Head] = null;
            _humanoidJoints[JointType.Neck] = SpineShoulder;

            _humanoidJoints[JointType.SpineShoulder] = SpineMid;
            _humanoidJoints[JointType.SpineMid] = SpineBase;
            _humanoidJoints[JointType.SpineBase] = null;

            _humanoidJoints[JointType.ShoulderRight] = ShoulderBaseRight;
            _humanoidJoints[JointType.ElbowRight] = ShoulderRight;
            _humanoidJoints[JointType.WristRight] = ElbowRight;
            //_humanoidJoints[JointType.HandRight] = HandRight;

            _humanoidJoints[JointType.ShoulderLeft] = ShoulderBaseLeft;
            _humanoidJoints[JointType.ElbowLeft] = ShoulderLeft;
            _humanoidJoints[JointType.WristLeft] = ElbowLeft;
            //_humanoidJoints[JointType.HandLeft] = HandLeft;

            _humanoidJoints[JointType.HipLeft] = null;
            _humanoidJoints[JointType.KneeLeft] = HipLeft;
            _humanoidJoints[JointType.AnkleLeft] = KneeLeft;

            _humanoidJoints[JointType.HipRight] = null;
            _humanoidJoints[JointType.KneeRight] = HipRight;
            _humanoidJoints[JointType.AnkleRight] = KneeRight;
        }

        public void Start()
        {
            Debug.Log("Start");
            InitializeKinect();
        }

        #region Helpers

        private void CreateJoints()
        {
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                var jointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                jointObject.transform.parent = gameObject.transform;
                jointObject.name = string.Format("Joint_{0}", jointType);
                jointObject.transform.localScale += new Vector3(18, 18, 18);

                _joints[jointType] = jointObject;
            }
        }

        private void CreateBones()
        {
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                var boneObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                boneObject.transform.parent = gameObject.transform;
                boneObject.name = string.Format("Bone_{0}", jointType);
                boneObject.transform.localScale += new Vector3(1, 10, 1);

                _bones[jointType] = boneObject;
            }
        }

        private void InitializeKinect()
        {
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

        private void Kinect_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                    return;

                if (_bodies == null)
                    _bodies = new Body[bodyFrame.BodyCount];

                // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                // As long as those body objects are not disposed and not set to null in the array,
                // those body objects will be re-used.
                bodyFrame.GetAndRefreshBodyData(_bodies);

                var trackedBodies = _bodies.Where(x => x.IsTracked).ToArray();

                if (!trackedBodies.Any())
                    return;

                var body = trackedBodies.First();

                DrawJoints(body);
                DrawBones(body);

                RotateHumanoid(body);
            }
        }

        private void RotateHumanoid(Body body)
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

                var rotationKinSys = body.JointOrientations[jointType].Orientation.ToUnity();

                var rotationUnitySys = new Quaternion
                (
                    rotationKinSys.x,
                    -rotationKinSys.y,
                    -rotationKinSys.z,
                    rotationKinSys.w
                );

                var humanoidRotation = Humanoid.transform.rotation;
                //var inversedHumanoidRotation = Quaternion.Inverse(humanoidRotation);

                Quaternion addRot;

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

                jointObject.transform.rotation = humanoidRotation * rotationUnitySys * addRot;

            }
        }

        private void DrawJoints(Body body)
        {
            //обрисовывем нашего человечка
            foreach (var joint in body.Joints.Values)
            {
                if (!_humanoidJoints.ContainsKey(joint.JointType))
                    continue;

                var humanoidPart = _humanoidJoints[joint.JointType];
                if (humanoidPart == null)
                    continue;

                var jointObject = _joints[joint.JointType];

                jointObject.transform.position = humanoidPart.transform.position;
                jointObject.transform.rotation = humanoidPart.transform.rotation;
            }

            //рисуем скелет в пространстве
            //foreach (var joint in body.Joints.Values)
            //{
            //    var jointPos = joint.Position;
            //    var jointObject = _joints[joint.JointType];

            //    jointObject.transform.localPosition = new Vector3(jointPos.X, jointPos.Y, jointPos.Z);
            //    jointObject.transform.localPosition *= PosK;

            //    var jointQuadrion = body.JointOrientations[joint.JointType].Orientation.ToUnity();
            //    jointObject.transform.rotation = jointQuadrion;
            //}
        }

        private void DrawBones(Body body)
        {
            Tuple[] pairs =
            {
                //new Tuple(JointType.Head, JointType.Neck, Head,false),
                //new Tuple(JointType.SpineShoulder, JointType.Neck, Neck, false),

                //new Tuple(JointType.SpineMid, JointType.SpineShoulder, SpineMid, false),
                //new Tuple(JointType.SpineBase, JointType.SpineMid, SpineBase, false),

                //new Tuple(JointType.ShoulderRight, JointType.ElbowRight, RightArm, false),
                //new Tuple(JointType.ElbowRight, JointType.HandRight, RightForeArm, false),

                //new Tuple(JointType.ShoulderLeft, JointType.ElbowLeft, LeftArm, false),
                //new Tuple(JointType.ElbowLeft, JointType.HandLeft, LeftForeArm, false),

                //new Tuple(JointType.SpineBase, JointType.HipLeft, LHipJoint, false),
                //new Tuple(JointType.HipLeft, JointType.KneeLeft, LeftUpLeg, false),

                //new Tuple(JointType.KneeLeft, JointType.AnkleLeft, LeftLeg, false),
                //new Tuple(JointType.AnkleLeft, JointType.FootLeft, LeftFoot, true),

                //new Tuple(JointType.SpineBase, JointType.HipRight, RHipJoint, false),
                //new Tuple(JointType.HipRight, JointType.KneeRight, RightUpLeg, false),

                //new Tuple(JointType.KneeRight, JointType.AnkleRight, RightLeg, false),
                //new Tuple(JointType.AnkleRight, JointType.FootRight, RightFoot, true),
            };

            foreach (var tuple in pairs)
            {
                DrawBone(body, tuple.First, tuple.Second, tuple.Third, tuple.UseCustomRotation);
            }
        }

        private void DrawBone(Body body, JointType startJointType, JointType endJointType, GameObject relatedGameObject, bool useCustomRotation)
        {
            const float boneWidthScale = 5;

            var jointOrientation = body.JointOrientations[endJointType];
            var boneObject = _bones[endJointType];

            var startJoint = body.Joints[startJointType];
            var endJoint = body.Joints[endJointType];

            var startJointPos = startJoint.Position.ToUnity() * PosK;
            var endJointPos = endJoint.Position.ToUnity() * PosK;

            var boneLength = Vector3.Distance(startJointPos, endJointPos);
            boneObject.transform.localScale = new Vector3(boneWidthScale, boneLength, boneWidthScale * 2);

            //var boneVector = jointOrientation.Orientation.ToUnity() * Vector3.up;
            //boneVector.Set(-boneVector.x, boneVector.y, boneVector.z);
            //var normalVector = jointOrientation.Orientation.ToUnity() * Vector3.forward;
            //var rot = Quaternion.FromToRotation(Vector3.up, boneVector);
            //var reverseRot = Quaternion.FromToRotation(boneVector, Vector3.up);

            //boneObject.transform.LookAt(normalVector);

            //Quaternion


            //Vector3 boneVector;//= endJointPos - startJointPos;

            //boneVector = jointOrientation.Orientation.ToUnity() * Vector3.up;
            //var normalVector = jointOrientation.Orientation.ToUnity() * Vector3.forward;

            Quaternion rotationKinSys;
            Quaternion rotationUnitySys;

            if (useCustomRotation)
            {
                var boneVector = endJointPos - startJointPos;


                //if (relatedGameObject == RightFoot)
                //{
                //    boneVector=new Vector3(boneVector.x*0, boneVector.y*0, boneVector.z * 1);
                //}


                rotationKinSys = Quaternion.FromToRotation(Vector3.up, boneVector);




            }
            else
            {
                rotationKinSys = jointOrientation.Orientation.ToUnity();
            }

            rotationUnitySys = new Quaternion
             (
                 rotationKinSys.x,
                 -rotationKinSys.y,
                 -rotationKinSys.z,
                 rotationKinSys.w
             ); ;


            boneObject.transform.rotation = rotationKinSys;
            boneObject.transform.localPosition = startJointPos + (endJointPos - startJointPos) / 2;

            if (relatedGameObject != null)
            {
                var humanoidRotation = new Quaternion();

                if (Humanoid != null)
                {
                    humanoidRotation = Humanoid.transform.rotation;
                }

                relatedGameObject.transform.rotation = rotationUnitySys * humanoidRotation;

                if (_initialYRotations.ContainsKey(relatedGameObject))
                {
                    var localYRotation = AdditionalRotationY;//_initialYRotations[relatedGameObject];
                    var currentAngles = relatedGameObject.transform.localEulerAngles;

                    relatedGameObject.transform.localEulerAngles = new Vector3(currentAngles.x - localYRotation, currentAngles.y, currentAngles.z);

                }




            }
        }



        #endregion
    }
}
