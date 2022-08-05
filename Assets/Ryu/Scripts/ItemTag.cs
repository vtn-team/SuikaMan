using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// アイテムを使用するコライダーにつけるコンポーネント
/// </summary>
public class ItemTag : MonoBehaviour
{
    [Tooltip("使用できるアイテムの種類"),SerializeField] private ItemType _tagType;
    // Start is called before the first frame update
    /// <summary>
    /// 呼び出されたときアイテムの種類を読み取れる
    /// </summary>
    public ItemType TagType
    {
        get { return _tagType; }
    }
}
