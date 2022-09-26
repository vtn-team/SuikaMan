using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    [Tooltip("周辺アイテムのリスト"), SerializeField] private List<GameObject> _surroundItems = new List<GameObject>();

    [Tooltip("クラフト素材のリスト"), SerializeField] private List<ItemType> _craftMaterials1 = new List<ItemType>();
    [Tooltip("クラフト後のアイテム"), SerializeField] private GameObject _craftItem1;
    CraftMechanism _crRaft;

    //[Tooltip("クラフト素材のリスト"), SerializeField] private List<ItemType> _craftMaterials2 = new List<ItemType>();
    //[Tooltip("クラフト後のアイテム"), SerializeField] private GameObject _craftItem2;
    //CraftMechanism _crKey;
    // Start is called before the first frame update
    void Start()
    {
        _crRaft = new CraftMechanism(_craftMaterials1,_craftItem1,0);
        //_crKey = new CraftMechanism(_craftMaterials2,_craftItem2,1);
    }

    // Update is called once per frame
    void Update()
    {
        //OVRコントローラーのBボタンが押されたとき
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            //周辺のアイテム数がクラフトの素材数より多いなら
            if (_craftMaterials1.Count <= _surroundItems.Count)
            {
                //周辺アイテムの照らし合わせを行う
                CheckList(_craftMaterials1,_craftItem1);
            }
            //周辺のアイテム数がクラフトの素材数より多いなら
            //if (_craftMaterials2.Count <= _surroundItems.Count)
            //{
            //    //周辺アイテムの照らし合わせを行う
            //    CheckList(_craftMaterials2,_craftItem2);
            //}
        }
        //確認用
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            foreach (var item in _surroundItems)
            {
                Debug.Log(item);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //コライダーに入ったオブジェクトのコンポーネントを取得
        var obj = other.gameObject.GetComponent<ItemMechanism>();
        
        //もしコンポーネントが取得できているなら
        if (obj)
        {
            Debug.Log($"Add:{other.gameObject.name}:({obj})");
            //リストに加える
            _surroundItems.Add(other.gameObject);
        }
        Debug.Log($"_周辺のアイテム数:{_surroundItems.Count}");
    }
    private void OnTriggerExit(Collider other)
    {
        //コライダーに入ったオブジェクトのコンポーネントを取得
        var obj = other.gameObject.GetComponent<ItemMechanism>();
        //もしコンポーネントが取得できているなら
        if (obj)
        {
            //リストから外す
            _surroundItems.Remove(other.gameObject);
        }
        Debug.Log($"_周辺のアイテム数:{_surroundItems.Count}");
    }
    /// <summary>
    /// クラフト後のアイテムを生成する処理
    /// </summary>
    public void CraftItem(GameObject item)
    {
        //プレイヤーの前方にクラフト後のアイテムを生成する
        Instantiate(item, transform.position += transform.forward
            , transform.rotation);
        //Instantiate(item, transform.position += transform.forward
        //  , transform.rotation);
    }
    //周辺のアイテムのリストを照合する処理
    public void CheckList(List<ItemType> array, GameObject craft)
    {
        //クラフト素材の種類の作業用リスト
        List<ItemType> useMaterial = new List<ItemType>();
        //周辺アイテムの種類を複製
        _surroundItems.ForEach(i => useMaterial.Add(i.GetComponent<ItemMechanism>().Type));
        //周辺オブジェクトの作業用リスト
        List<GameObject> useObject = new List<GameObject>();
        //周辺オブジェクトを複製
        _surroundItems.ForEach(i => useObject.Add(i));
        //クラフト可能かどうか 
        bool canCraft = true;
        //クラフト素材を全て確認するとき
        foreach(var item in array)
        {
            //現在のアイテムが存在するかどうか
            bool itemFind = false;
            //周辺アイテムの数だけ確認するとき
            for(int i = 0; i < useMaterial.Count; i++)
            {
                //周辺アイテムのi番目と現在のアイテムが違うなら
                if (useMaterial[i] != item)
                {
                    //iを進める
                    continue;
                }
                //同じときは
                Debug.Log($"一致：{useMaterial[i]}");
                //i番目のアイテムの種類を無効にする
                useMaterial[i] = ItemType.Invalid;
                //現在のアイテムが存在する判定
                itemFind = true;
                //現在のアイテムを進める
                break;
            }
            //アイテムが存在しないとき
            if (!itemFind)
            {
                //クラフト不可能な判定
                canCraft = false;
                //確認を終える
                break;
            }
        }
        //クラフトが可能な時
        if (canCraft)
        {
            Debug.Log("Craft");
            //クラフト時の素材の処理を行う
            MaterialProcess(useMaterial,useObject,craft);
        }

        ////一致しているアイテムの要素番号
        //int[] checkNumber = new int[100];
        ////一致しているアイテムの数
        //int checkCount = 0;
        ////クラフト素材の要素数の回数繰り返す
        //for (int c = 0; c < _craftMaterials.Count; c++)
        //{
        //    //周辺のアイテムの要素数の回数繰り返す
        //    for (int s = 0; s < _surroundingItems.Count; s++)
        //    {
        //        //周辺のアイテムの種類を取得
        //        var item = _surroundingItems[s].GetComponent<ItemMechanism>().Type;
        //        //クラフト素材のリストの種類と周辺アイテムのリストの種類が同じとき
        //        if (_craftMaterials[c] == item)
        //        {
        //            bool isExist = false;
        //            for(int i = 0; i < checkNumber.Length; i++)
        //            {
        //                if (checkNumber[i] == s)
        //                {
        //                    isExist = true;
        //                }
        //            }
        //            if (!isExist)
        //            {
        //                //周辺アイテムの要素数を記憶
        //                checkNumber[c] = s;
        //                checkCount++;
        //                Debug.Log($"s:{s}");
        //                break;
        //            }
        //        }

        //    }
        //}
        //Debug.Log($"count:{checkCount}");
        ////一致数とクラフト素材の要素数が同じとき
        //if (_craftMaterials.Count == checkCount)
        //{
        //    //アイテムを生成する
        //    CraftItem();
        //    //一致している数分の周辺アイテムを消費する
        //    for (int i = 0; i < checkCount; i++)
        //    {
        //        _surroundingItems[checkNumber[i]].SetActive(false);
        //        _surroundingItems.RemoveAt(checkNumber[i]);
        //    }
        //}
    }
    /// <summary>
    /// クラフト時の素材の処理
    /// </summary>
    /// <param name="useMaterial">周辺アイテムの種類のリスト</param>
    /// <param name="useObject">周辺アイテムのオブジェクトのリスト</param>
    private void MaterialProcess(List<ItemType> useMaterial,List<GameObject> useObject, GameObject craft)
    {
        //クラフトの生成処理をする
        CraftItem(craft);
        //周辺のオブジェクトのリストを全て確認するとき
        foreach(var item in useObject)
        {
            //アイテムを非表示にする
            item.gameObject.SetActive(false);
        }
        //周辺のアイテムのリストを空にする
        _surroundItems.Clear();
        //周辺のアイテムの種類を確認するとき
        for(int i = 0; i < useMaterial.Count; i++)
        {
            //周辺のアイテムの種類のi番目が無効なら、iを進める
            if (useMaterial[i] == ItemType.Invalid) continue;
            //無効でないなら周辺のアイテムのi番目をリストに戻す
            _surroundItems.Add(useObject[i]);
            //i番目のオブジェクトを表示する
            useObject[i].gameObject.SetActive(true);
        }
    }
}

public class CraftMechanism
{
    private List<ItemType> craftMaterials;
    private GameObject craftItem;
    private int priority;
    public CraftMechanism(List<ItemType> materials, GameObject item, int priority)
    {
        craftMaterials = materials;
        craftItem = item;
        this.priority = priority;
    }
}