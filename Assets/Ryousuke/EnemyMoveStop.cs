using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveStop : MonoBehaviour
{
    public bool stop = false;
    
     IEnumerator Stop()
    {
        stop = true;

        yield return new WaitForSeconds(5);

        stop = false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            StartCoroutine(Stop());
        }
        
    }

}
