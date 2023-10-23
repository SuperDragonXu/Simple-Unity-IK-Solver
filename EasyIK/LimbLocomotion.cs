using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyIK;
public class LimbLocomotion : MonoBehaviour
{
    public int limbJointCount = 3;
    public bool lazyBones;
    public float stepLength = 1;
    [Range(0,1)]
    public float weight = 1;
    public Transform Pole;
    public List<Transform> limbEnds;
    public float limbUpdateSmoothness = 1;

    private List<FabricIK_Solver> limbIKs;
    private List<Transform> ikTargets;
    private List<Vector3> targetsTargetPos;


    private List<Vector3> limbOffsets;
    void Start()
    {
        limbOffsets = new();
        limbIKs = new();
        ikTargets = new();
        targetsTargetPos = new();
        foreach(var item in limbEnds)
        {
            FabricIK_Solver ik = item.gameObject.AddComponent<FabricIK_Solver>();
            ik.lazyBones = lazyBones;
            ik.bones = new();
            ik.bones.Add(item);
            ik.boneCount = limbJointCount;
            ik.weight = weight;
            if(Pole!= null)
                ik.Pole = Pole;

            limbIKs.Add(ik);

            Vector3 offset = item.position - transform.position;
            limbOffsets.Add(offset);

            GameObject target= new ("ik target");
            target.transform.position = item.position;
            ikTargets.Add(target.transform);
            targetsTargetPos.Add(target.transform.position);
            ik.Target = target.transform;
            //ik.activated = true;
            ik.Init();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        UpdateAllIKTargets();
    }
    private void UpdateAllIKTargets()
    {
        for(int i = 0; i < limbEnds.Count; i++)
        {
            float horizontalDis = Vector2.SqrMagnitude(new Vector2(targetsTargetPos[i].x - ikTargets[i].position.x, targetsTargetPos[i].z - ikTargets[i].position.z));
            Vector3 targetpos = new Vector3(targetsTargetPos[i].x, (targetsTargetPos[i].y + horizontalDis* 1f) , targetsTargetPos[i].z);
            ikTargets[i].position = Vector3.Lerp(ikTargets[i].position, targetpos, Time.deltaTime * limbUpdateSmoothness);

            if(Vector3.Distance(targetsTargetPos[i], transform.position + transform.rotation * limbOffsets[i]/*limbEnds[i].position*/) > stepLength)
            {
                UpdateOneLimbTarget(i, transform.position + transform.rotation * limbOffsets[i] - targetsTargetPos[i]);

            }
            SolveIK(i);
        }
        
    }
    private void UpdateOneLimbTarget(int index, Vector3 offset)
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + transform.rotation * limbOffsets[index] + transform.up + offset.normalized * stepLength * 0.5f,transform.up*(-1), out hit))
        {
            targetsTargetPos[index]= hit.point;

        }
    }
    private void SolveIK(int index)
    {
        limbIKs[index].Solve();
    }
}
