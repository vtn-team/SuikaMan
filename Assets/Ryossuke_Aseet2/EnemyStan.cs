using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStan : Enemy
{
    //ƒJƒƒ‰‚ªŒ©‚Â‚©‚Á‚½‚çŒãX“ü‚ê‚é
     
    public bool isStop;

     void Start()
    {
        isStop = false;
    }

     void Update()
    {
        if(!isStop)
        {
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isStop = true;
    }
}
