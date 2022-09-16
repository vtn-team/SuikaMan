using UnityEngine.AI;
using UnityEngine;
/// <summary>
/// ナビメッシュを使うとき用のEnemy
/// </summary>
public class SampleEnemy : MonoBehaviour
{
    [SerializeField, Tooltip("徘徊してほしい場所")] Transform[] _wanderingPoint;
    [SerializeField, Tooltip("徘徊してほしい場所")] int destPoint = 0;
    [SerializeField, Tooltip("追いかけるプレイヤー")] GameObject _playerObj;
    [Tooltip("ナビメッシュ")] NavMeshAgent m_agent;
    [Tooltip("徘徊するBool")] bool _wanderingBool = true;

    private RaycastHit hit;

    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.autoBraking = false;

        GotoNextPoint();
    }

    void Update()
    {
        if (_wanderingBool == true)
        {
            if (!m_agent.pathPending && m_agent.remainingDistance < 0.5f)
            {
                GotoNextPoint();
            }
        }
    }

    void GotoNextPoint()
    {

        Debug.Log(_wanderingBool);
        // 地点がなにも設定されていないときに返します
        if (_wanderingPoint.Length == 0)
            return;

        // エージェントが現在設定された目標地点に行くように設定します
        m_agent.destination = _wanderingPoint[destPoint].position;

        // 配列内の次の位置を目標地点に設定し、
        // 必要ならば出発地点にもどります
        destPoint = (destPoint + 1) % _wanderingPoint.Length;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == _playerObj)
        {
            var diff = _playerObj.transform.position - transform.position;
            var distance = diff.magnitude;
            var direction = diff.normalized;

            if (Physics.Raycast(transform.position, direction, out hit, distance))
            {
                Debug.Log("Rayが当たった");
                _wanderingBool = false;
                m_agent.isStopped = false;
                m_agent.destination = _playerObj.transform.position;
            }
            else
            {
                m_agent.isStopped = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _wanderingBool = true; ;
    }
}
