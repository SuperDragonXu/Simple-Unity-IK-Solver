using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCDIK_Solver : MonoBehaviour
{
    [Range(0, 1)]
    public float weight;

    public bool lazyBones;

    public int iterations = 10;
    public List<Transform> bones;   
    public int boneCount;
    public List<float> boneLength;

    public Transform Target;
    public bool rotateWithTarget;

    public Transform Pole;

    private Vector3 targetPos;

    private void OnEnable()
    {
        InitBoneLength();        
    }

    /// <summary>
    /// Initialize boneLength List
    /// </summary>
    private void InitBoneLength()
    {
        if (boneCount == 0)
        {
            boneCount = bones.Count;
            boneLength = new List<float>();
            for (int i = 0; i < bones.Count - 1; i++)
            {
                boneLength.Add(Vector3.Distance(bones[i].position, bones[i + 1].position));
            }
        }
        else if (boneCount >= 1 && bones.Count == 1)
        {
            var currentBone = bones[0];
            for (int i = 1; i < boneCount; i++)
            {
                bones.Add(currentBone.parent);

                boneLength.Add(Vector3.Distance(bones[i].position, bones[i - 1].position));
                currentBone = currentBone.parent;
            }
        }
    }


    /// <summary>
    /// A interpolation to calculate the target position according to the weight, target Transform, and end bone position.
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
        if (!CalculateTargetPos())
            return;
        
        IK_SolveIterations();

        //MoveToPole();
    }

    /// <summary>
    /// CCD
    /// </summary>
    private void IK_SolveIterations()
    {
        for(int i = 0; i < iterations; i ++)
        {
            if(!lazyBones)
            {
                for(int k = 0; k <boneCount;k++)
                {
                    bones[k].rotation = Quaternion.FromToRotation(bones[0].position - bones[k].position, targetPos - bones[k].position) * bones[k].rotation;
                }
            }
            else
            {
                for (int j = 2; j < boneCount; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {                        
                        bones[k].rotation = Quaternion.FromToRotation(bones[0].position - bones[k].position, targetPos - bones[k].position) * bones[k].rotation;

                        if(Pole!=null && k<boneCount - 2)
                        {
                            var plane = new Plane(bones[k + 2].position - bones[k].position, bones[k].position);
                            var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                            var projectedBone = plane.ClosestPointOnPlane(bones[k + 1].position);
                            var angle = Vector3.SignedAngle(projectedBone - bones[k].position, projectedPole - bones[k].position, plane.normal);
                            bones[k].rotation = Quaternion.AngleAxis(angle, plane.normal) * bones[k].rotation;
                        }                        
                    }
                }
            }            
        }
    }

    private void MoveToPole()
    {
        if (Pole == null)
            return;
        for (int i = 0; i < boneCount - 2; i++)
        {
            var plane = new Plane(bones[i + 2].position - bones[i].position, bones[i].position);
            var projectedPole = plane.ClosestPointOnPlane(Pole.position);
            var projectedBone = plane.ClosestPointOnPlane(bones[i + 1].position);
            var angle = Vector3.SignedAngle(projectedBone - bones[i].position, projectedPole - bones[i].position, plane.normal);
            bones[i].rotation = Quaternion.AngleAxis(angle, plane.normal)* bones[i].rotation;
        }
    }
}
