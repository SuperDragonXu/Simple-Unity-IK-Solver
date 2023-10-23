using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIK
{
    public class FabricIK_Solver : MonoBehaviour
    {
        public bool activated = false;
        [Range(0,1)]
        public float weight;

        public bool lazyBones;

        public int iterations = 10;
        public List<Transform> bones;
        private List<Vector3> copiedBonePositions;
        private List<Quaternion> StartBoneRotations;
        private List<Vector3> StartBoneDirections;
        public int boneCount;
        [HideInInspector]
        public List<float> boneLength;

        public Transform Target;
        public bool rotateWithTarget;

        public Transform Pole;

        private Vector3 targetPos;
        Transform boneRoot;
        public void OnEnable()
        {
            if (!activated)
                return;
            InitBoneLength();

            CalculateTargetPos();
            CopyBonePositionsAndRotations();
            InitStartBone();
        }
        public void Init()
        {
            InitBoneLength();

            CalculateTargetPos();
            CopyBonePositionsAndRotations();
            InitStartBone();

            boneRoot = bones[bones.Count - 1];
        }
        /// <summary>
        /// Initialize boneLength List
        /// </summary>
        private void InitBoneLength()
        {
            boneLength = new();
            if(boneCount == 0)
            {
                boneCount = bones.Count;
                boneLength = new List<float>();
                for (int i = 0; i < bones.Count - 1; i++)
                {
                    boneLength.Add(Vector3.Distance(bones[i].position, bones[i + 1].position));
                }
            }
            else if(boneCount >= 1 && bones.Count == 1)
            {                
                var currentBone = bones[0];
                for(int i = 1; i < boneCount; i++)
                {
                    bones.Add(currentBone.parent);                    
                    //Debug.Log(i + " " + (i - 1));
                    boneLength.Add(Vector3.Distance(bones[i].position, bones[i - 1].position));
                    currentBone = currentBone.parent;
                }
            }
        }

        private void CopyBonePositionsAndRotations()
        {
            copiedBonePositions = new();
           
            

            for (int i = 0; i < boneCount; i++)
            {
                copiedBonePositions.Add(GetPositionRootSpace(bones[i]));
            }            
        }
        private void InitStartBone()
        {
            StartBoneRotations = new();
            StartBoneDirections = new();

            StartBoneDirections.Add(targetPos - GetPositionRootSpace(bones[0]));
            for (int i = 0; i < boneCount; i++)
            {                
                StartBoneRotations.Add(GetRotationRootSpace(bones[i]));//GetWorldRotation(bones[i].rotation));
                if (i < boneCount - 1)
                    StartBoneDirections.Add( GetPositionRootSpace(bones[i]) - GetPositionRootSpace(bones[i + 1]));
            }
        }

        /// <summary>
        /// calculate the target position according to the weight, target Transform, and end bone position.
        /// </summary>
        private bool CalculateTargetPos()
        {
            if (Target == null)
            {
                Debug.LogError("Target gameobject of the faric IK is not assigned! Please assign a target object.");
                return false;
            }
            targetPos = Vector3.Lerp(GetPositionRootSpace(bones[0]),  GetPositionRootSpace(Target), weight);
            return true;
        }

        private void LateUpdate()
        {
            if (!activated)
                return;
            Solve();
        }
        public void Solve()
        {
            
            CopyBonePositionsAndRotations();
            if (!CalculateTargetPos())
                return;

            IK_SolveIterations();

            MoveToPole();
            PasteBones();
        }

        private void IK_SolveIterations()
        {
            for (int i = 0; i < iterations; i++)
            {
                if (lazyBones)
                {
                    for (int j = 2; j < boneCount; j++)
                    {
                        var rootPos = GetPositionRootSpace(bones[j]);
                        ForwardIterations(j);
                        BackwardIterations(j, rootPos);

                    }
                }
                else
                {
                    var rootPos = GetPositionRootSpace(bones[boneCount - 1]);
                    ForwardIterations(boneCount - 1);
                    BackwardIterations(boneCount - 1, rootPos);
                }

            }
        }

        private void MoveToPole()
        {
            if (Pole == null)
                return;
            for (int i = 1; i < boneCount - 1; i++)
            {
                var plane = new Plane(copiedBonePositions[i + 1] - copiedBonePositions[i - 1], copiedBonePositions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(GetPositionRootSpace(Pole));
                var projectedBone = plane.ClosestPointOnPlane(copiedBonePositions[i]);
                var angle = Vector3.SignedAngle(projectedBone - copiedBonePositions[i - 1], projectedPole - copiedBonePositions[i - 1], plane.normal);
                copiedBonePositions[i] = Quaternion.AngleAxis(angle, plane.normal) * (copiedBonePositions[i] - copiedBonePositions[i - 1]) + copiedBonePositions[i - 1];
            }
        }

        private void ForwardIterations(int cnt)
        {
            copiedBonePositions[0] = targetPos;
            if (cnt < 2)
                return;
            
            for (int i = 1; i < cnt; i++)
            {
                var dir = (copiedBonePositions[i] - copiedBonePositions[i - 1]).normalized;
                copiedBonePositions[i] = copiedBonePositions[i - 1] + dir * boneLength[i - 1];                
            }
        }

        private void BackwardIterations(int cnt, Vector3 rootPos)
        {
            copiedBonePositions[cnt] = rootPos;
            if (boneCount < 2 || cnt < 2)
                return;
            for (int i = cnt - 1; i >= 0; i--)
            {
                var dir = (copiedBonePositions[i] - copiedBonePositions[i + 1]).normalized;
                copiedBonePositions[i] = copiedBonePositions[i + 1] + dir * boneLength[i];                
            }
        }

        private void PasteBones()
        {
            for (int i = boneCount - 1; i > 0; i--)
            {

                SetRotationRootSpace( bones[i], Quaternion.FromToRotation(StartBoneDirections[i], copiedBonePositions[i - 1] - copiedBonePositions[i]) * StartBoneRotations[i]);

                SetPositionRootSpace(bones[i],copiedBonePositions[i]);

            }
            SetPositionRootSpace(bones[0], copiedBonePositions[0]);
            if (rotateWithTarget)
                //SetRotationRootSpace( bones[0], GetRotationRootSpace(Target));//Quaternion.FromToRotation(StartBoneDirections[0], Target.position - copiedBonePositions[0]) * StartBoneRotations[0];
                bones[0].rotation = Target.rotation;
        }

        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (boneRoot == null)
                return current.position;
            else
                return Quaternion.Inverse(boneRoot.parent.rotation) * (current.position - boneRoot.parent.position);
        }

        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (boneRoot == null)
                current.position = position;
            else
                current.position = boneRoot.parent.rotation * position + boneRoot.parent.position;
        }

        private Quaternion GetRotationRootSpace(Transform current)
        {
            //inverse(after) * before => rot: before -> after
            if (boneRoot == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * boneRoot.parent.rotation;
        }

        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (boneRoot == null)
                current.rotation = rotation;
            else
                current.rotation = boneRoot.parent.rotation * rotation;
        }
    }
}

