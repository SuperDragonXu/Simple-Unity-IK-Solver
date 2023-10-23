using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balancer : MonoBehaviour
{
    public Transform body;    

    // Update is called once per frame
    void Update()
    {
        transform.position = body.position;
    }
}
