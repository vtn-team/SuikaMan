using UnityEngine;
using UnityEngine.AI;

public class Enemy : EnemyMoveStop
{
    [SerializeField, Tooltip("追いかける相手")] GameObject _target;
    [SerializeField,Tooltip("エネミーのスピード")] float _speed = 0.1f;
    float distance;
    

    /// <summary>徘徊してほしい場所</summary>
    [SerializeField] Transform[] _wanderingPoint;
    [SerializeField] int destPoint = 0;
    bool _enemy = false;
   
    void Start()
    {
        GotoNextPoint();
    }

    void Update()
    {
        if(stop == true)
        {
            return;
        }
            

        var playerPos = _target.transform.position;
        distance = Vector3.Distance(this.transform.position, playerPos);

        if (distance < 15)
        {
            //targetの方に少しずつ向きが変わる
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_target.transform.position - transform.position), 0.3f);

            //targetに向かって進む
            transform.position += transform.forward * _speed;
        }
        else
        {
            GotoNextPoint();
            //if(_enemy == false) { GotoNextPoint(); }
        }
    }

    void GotoNextPoint()
    {
        // 地点がなにも設定されていないときに返します
        if (_wanderingPoint.Length == 0)
            return;
;
        //targetの方に少しずつ向きが変わる
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_wanderingPoint[destPoint].position - transform.position), 0.3f);
         //targetに向かって進む
        transform.position += transform.forward * _speed;
        //_enemy = false;

        //// 配列内の次の位置を目標地点に設定し、
        //// 必要ならば出発地点にもどります
        //destPoint = (destPoint + 1) % _wanderingPoint.Length;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == _target)
        {
            Debug.Log($"スイカ男が捕まえた");
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Item")
        {
            // 配列内の次の位置を目標地点に設定し、
            // 必要ならば出発地点にもどります
            destPoint = (destPoint + 1) % _wanderingPoint.Length;
            //GotoNextPoint();

        }
    }
}
