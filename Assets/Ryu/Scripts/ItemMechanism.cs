using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// プレイヤーが使用できるアイテムに付けるコンポーネント
/// </summary>
public class ItemMechanism : MonoBehaviour
{
    //OVRの掴まれるオブジェクトに付けるコンポーネント
    private OVRGrabbable _grabbable;

    [Tooltip("アイテムの種類"),SerializeField] private ItemType _itemType;
    [Tooltip("使用可能判定"),SerializeField] private bool _isUsable = false;

    /// <summary>
    /// アイテムの種類を読み取るのみ
    /// </summary>
    public ItemType Type
    {
        get => _itemType;
    }

    void Start()
    {
        //OVRGrabbableを取得する
        _grabbable = GetComponent<OVRGrabbable>();

        //アイテムがカメラならば
        if (Type == ItemType.Camera)
        {
            //使用可能にする
            _isUsable = true;
        }
    }

 
    void Update()
    {
        //手に持ちながら、「OculusコントローラーのAボタン」または「左クリック」をしたとき
        if (_grabbable.isGrabbed == true &&OVRInput.GetDown(OVRInput.Button.One)||Input.GetButtonDown("Fire1"))
        {
            //使用可能なら
            if (_isUsable == true)
            {
                //アイテムを使う
                UseItem();

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //アイテム使用エリアの判別用クラスを取得する
        var type = other.GetComponent<ItemTag>();
        //判別用クラスが取得できている場合
        if (type != null)
        {
            Debug.Log(type.TagType);

            //アイテムの種類と使用エリアのタイプが対応しているとき
            if (_itemType == type.TagType)
            {
                //アイテムを使用可能にする
                _isUsable = true;
                Debug.Log($"使用可能:{Type}");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //アイテム使用エリアの判別用クラスを取得する
        var type = other.GetComponent<ItemTag>();
        //判別用クラスが取得できている場合
        if (type != null)
        {
            //このアイテムの種類と判別用クラスの種類が同じなら
            if (_itemType == type.TagType)
            {
                //アイテムを使用不可能にする
                _isUsable = false;
                Debug.Log($"使用不可能:{Type}");
            }
        }
    }
    //アイテムを使用する時の処理
    public void UseItem()
    {
        //アイテムを消費する
        Destroy(gameObject);
        Debug.Log("アイテムを使った");
    }
}
