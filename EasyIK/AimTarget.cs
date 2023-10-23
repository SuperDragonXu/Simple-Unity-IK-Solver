using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EasyIK
{
    public class AimTarget : MonoBehaviour
    {
        bool activated;
        [Range(0, 1)]
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

        public Transform boneTarget;
        public Transform Target;
        public bool rotateWithTarget;

        public Transform Pole;
        

        private Transform boneRoot;
        private Vector3 startTargetToRoot;
        private Vector3 startBoneTargetToRoot;
        private Vector3 BoneTargetStartPos;
        private float targetDis;

        private Quaternion targetStartRotation;

        FabricIK_Solver ik;

        private void Start()
        {
            ik = gameObject.AddComponent<FabricIK_Solver>();
            ik.weight = weight;
            ik.lazyBones = lazyBones;
            ik.iterations = iterations;
            foreach(var item in bones)
            {
                ik.bones = new();
                ik.bones.Add(item);
            }
            ik.boneCount = boneCount;
            ik.Target = boneTarget;
            ik.rotateWithTarget = rotateWithTarget;
            if(Pole!=null)
                ik.Pole = Pole;

            ik.Init();
            ik.activated = true;

            boneRoot = ik.bones[ik.bones.Count - 1];
            targetDis = Vector3.Distance(boneTarget.position, boneRoot.position);
            //startBoneTargetToRoot = boneTarget.position - boneRoot.position;
            //BoneTargetStartPos = boneTarget.position;

            startBoneTargetToRoot = GetPositionRootSpace(boneTarget) - GetPositionRootSpace(boneRoot);
            BoneTargetStartPos = GetPositionRootSpace(boneTarget);

            boneTarget.parent = Target;

            StartCoroutine(initDir());
        }
        private void FixedUpdate()
        {
            if (!activated)
                return;
            //Quaternion rotation = Quaternion.FromToRotation(startTargetToRoot,  Target.position - boneRoot.position);
            Quaternion rotation = Quaternion.FromToRotation(startTargetToRoot, GetPositionRootSpace(Target)- GetPositionRootSpace(boneRoot));
            var dir = rotation * startBoneTargetToRoot;
            dir = dir.normalized;            
            SetPositionRootSpace(boneTarget, GetPositionRootSpace(boneRoot)+ dir * targetDis);            
        }
        
        public void SetActivate(bool activate)
        {
            activated = activate;
            ik.activated = activate;
        }

        private IEnumerator initDir()
        {
            yield return new WaitForEndOfFrame();

            //startTargetToRoot =  Target.position - boneRoot.position;
            startTargetToRoot = GetPositionRootSpace(Target) - GetPositionRootSpace(boneRoot);

            //targetStartRotation = Target.rotation;
            targetStartRotation = GetRotationRootSpace(Target);
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