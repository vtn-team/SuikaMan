using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Gole : MonoBehaviour
{
    [SerializeField] GameObject _enemyObj = null;
    [SerializeField] GameObject _goleTimeLineObj = null;
    PlayableDirector _goleTimeLine = null;

    private void Awake()
    {
        _goleTimeLine = _goleTimeLineObj.GetComponent<PlayableDirector>();
        _goleTimeLine.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.CompareTag("Player")) { return; }
        var enemyPos = other.transform.position;
        enemyPos.z -= 10;
        _enemyObj.transform.position = enemyPos;

        _goleTimeLine.enabled = true;
    }
}
