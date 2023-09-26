using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIK
{
    public class FabricIK_Solver : MonoBehaviour
    {
        [Range(0,1)]
        public float weight;

        public bool lazyBones;

        public int iterations = 10;
        public List<Transform> bones;
        private List<Vector3> copiedBonePositions;
        private List<Quaternion> StartBoneRotations;
        private List<Vector3> StartBoneDirections;
        public int boneCount;
        public List<float> boneLength;

        public Transform Target;
        public bool rotateWithTarget;

        public Transform Pole;

        private Vector3 targetPos;

        private void OnEnable()
        {
            InitBoneLength();

            CalculateTargetPos();
            CopyBonePositionsAndRotations();
            InitStartBone();
        }

        /// <summary>
        /// Initialize boneLength List
        /// </summary>
        private void InitBoneLength()
        {
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
                copiedBonePositions.Add(bones[i].position);                
            }            
        }
        private void InitStartBone()
        {
            StartBoneRotations = new();
            StartBoneDirections = new();

            StartBoneDirections.Add(targetPos - bones[0].position);
            for (int i = 0; i < boneCount; i++)
            {                
                StartBoneRotations.Add(bones[i].rotation);//GetWorldRotation(bones[i].rotation));
                if (i < boneCount - 1)
                    StartBoneDirections.Add(bones[i].position - bones[i + 1].position);
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
            targetPos = Vector3.Lerp(bones[0].position, Target.position, weight);
            return true;
        }

        private void LateUpdate()
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
                        var rootPos = bones[j].position;
                        ForwardIterations(j);
                        BackwardIterations(j, rootPos);

                    }
                }
                else
                {
                    var rootPos = bones[boneCount - 1].position;
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
                var projectedPole = plane.ClosestPointOnPlane(Pole.position);
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
            for(int i = boneCount - 1; i > 0; i --)
            {
                                  
                    bones[i].rotation = Quaternion.FromToRotation(StartBoneDirections[i], copiedBonePositions[i - 1] - copiedBonePositions[i]) * StartBoneRotations[i];                
                
                bones[i].position = copiedBonePositions[i];
                
            }
            bones[0].position = copiedBonePositions[0];
            if (rotateWithTarget)
                bones[0].rotation = Target.rotation;//Quaternion.FromToRotation(StartBoneDirections[0], Target.position - copiedBonePositions[0]) * StartBoneRotations[0];
        }        
    }
}

