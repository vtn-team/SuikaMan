using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanEnemy : MonoBehaviour
{
    public bool stop = false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Enemy1")
        {
            StartCoroutine(Stop());
        }
    }
    IEnumerator Stop()
    {
        stop = true;
        yield return new WaitForSeconds(5);
        stop = false;
        this.gameObject.SetActive(false);
    }
}
