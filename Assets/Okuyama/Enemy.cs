using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField, Tooltip("追いかける相手")] GameObject _target;
    [SerializeField,Tooltip("エネミーのスピード")] float _speed = 0.1f;

    void Start()
    {

    }

    void Update()
    {
        //targetの方に少しずつ向きが変わる
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_target.transform.position - transform.position), 0.3f);

        //targetに向かって進む
        transform.position += transform.forward * _speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == _target)
        {
            Debug.Log($"スイカ男が捕まえた");
        }
    }
}
