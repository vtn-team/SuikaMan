using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Tooltip("プレイヤーのスピード")] float _speed;
    int _itemCount;//回収の動作確認用
    Rigidbody _rb;

    void Start()
    {
       _rb = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 入力された方向を「カメラを基準とした XZ 平面上のベクトル」に変換する
        Vector3 dir = new Vector3(h, 0, v);
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0;

        // キャラクターを「入力された方向」に向ける
        if (dir != Vector3.zero)
        {
            transform.forward = dir;
            dir = transform.forward;
        }

        // Y 軸方向の速度を保ちながら、速度ベクトルを求めてセットする
        Vector3 velocity = dir.normalized * _speed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.name == "Item")
        {
            Debug.Log($"アイテムゲット ");
            Destroy(collision.gameObject);
            _itemCount++;
        }
        if (_itemCount == 4)
        {
            Debug.Log($"アイテムすべてゲット");
        }
    }
}
