using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyLimbs : MonoBehaviour
{
    [SerializeField] Transform copied;
    [SerializeField] Transform copyFrom;
    [SerializeField] ConfigurableJoint joint;

    Quaternion targetInitialRotation;
    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        copied = FindUtils.FindMyChild(copyFrom,name);
        targetInitialRotation = copied.transform.rotation;//copied.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        if(copied != null)
        {
            ConfigurableJointExtensions.SetTargetRotation(joint, copied.rotation, targetInitialRotation);
        }
        else
        {
            Debug.Log("Cant find bone to copy from");
        }
    }

    private Quaternion copyRotation()
    {
        return Quaternion.Inverse(copied.localRotation) * targetInitialRotation;
    }
}
