using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// アイテムを使用するコライダーにつけるコンポーネント
/// </summary>
public class ItemTag : MonoBehaviour
{
    [Tooltip("使用できるアイテムの種類"),SerializeField] private ItemType _tagType;
    [Tooltip("次のフェーズで使うアイテム"),SerializeField] private GameObject _gameObject;
    /// <summary>
    /// 呼び出されたときアイテムの種類を読み取れる
    /// </summary>
    public ItemType TagType
    {
        get { return _tagType; }
    }

    public void AdvancePhase()
    {
        if (_gameObject != null)
        {
            Instantiate(_gameObject,new Vector3(transform.position.x, transform.position.y+3, transform.position.z),transform.rotation);
            Destroy(this.gameObject);
        }
        if(TagType == ItemType.Key)
        {
            GameClear();
        }
    }
    public void GameClear()
    {
        Debug.Log("Game Clear");
    }
}
