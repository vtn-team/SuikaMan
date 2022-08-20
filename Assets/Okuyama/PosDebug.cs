using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PosDebug : MonoBehaviour
{
    OVRGrabbable ovrGrabbable;
    public bool itemBool = false;

    private void Start()
    {
        ovrGrabbable = GetComponent<OVRGrabbable>();
    }
    private void Update()
    {
        if (ovrGrabbable.isGrabbed)
        {
            itemBool = true;
            //StartCoroutine(IsGrabbed());
        }
        else
        { 
            StartCoroutine(IsGrabbed());
        }
    }
    IEnumerator IsGrabbed()
    {
        yield return new WaitForSeconds(2);
        itemBool = false;
    }
}
