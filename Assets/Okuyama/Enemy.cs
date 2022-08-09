using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField, Tooltip("追いかける相手")] GameObject _target;
    [SerializeField,Tooltip("エネミーのスピード")] float _speed = 0.1f;
    [SerializeField] StanEnemy _stanEnemy;
    float distance;
    /// <summary>徘徊してほしい場所</summary>
    [SerializeField] Transform[] _wanderingPoint;
    [SerializeField] int destPoint = 0;
    bool _enemy = true;
   
    void Start()
    {
        GotoNextPoint();
    }

    void Update()
    {
        if (_stanEnemy.stop) { return; }
        var playerPos = _target.transform.position;
        distance = Vector3.Distance(this.transform.position, playerPos);

        //if (distance < 15)
        //{
        //    //targetの方に少しずつ向きが変わる
        //    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_target.transform.position - transform.position), 0.3f);

        //    //targetに向かって進む
        //    transform.position += transform.forward * _speed;
        //}
        if(_enemy == true)
        {
            GotoNextPoint();
        }
        if(distance < 15)
        {
            _enemy = false;
            //targetの方に少しずつ向きが変わる
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_target.transform.position - transform.position), 0.3f);

            //targetに向かって進む
            transform.position += transform.forward * _speed;
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

        //// 配列内の次の位置を目標地点に設定し、
        //// 必要ならば出発地点にもどります
        //destPoint = (destPoint + 1) % _wanderingPoint.Length;
    }
   
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "aa")
        {
            // 配列内の次の位置を目標地点に設定し、
            // 必要ならば出発地点にもどります
            destPoint = (destPoint + 1) % _wanderingPoint.Length;
            GotoNextPoint();
            _enemy = false;
        }
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log($"スイカ男が捕まえた");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "aa") { _enemy = true; }
    }
}
