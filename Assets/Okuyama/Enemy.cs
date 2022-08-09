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
    //加筆：笠
    [SerializeField] private int _nowPoint = 0; 
    public bool _enemy = true;
   
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
        //加筆：笠
        if(_enemy == false&&distance >= 15)
        {
            //徘徊してほしい座標に少しずつ向きが変わる
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_wanderingPoint[_nowPoint].transform.position - transform.position), 0.3f);
            //徘徊してほしい座標に向かって進む
            transform.position += transform.forward * _speed;
            //座標との距離の大きさが3以内になったら
            if((_wanderingPoint[_nowPoint].transform.position - transform.position).magnitude <= 3)
            {
                //次の座標に変える
                _nowPoint++;
            }
            //最後の座標に到達したら始めの座標に向かう
            if (_nowPoint > _wanderingPoint.Length-1 )
            {
                _nowPoint = 0;
            }
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
