using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabObj : MonoBehaviour
{
    [SerializeField, Tooltip("動いた差分")] float _sabun = 0.6f;
    [SerializeField, Tooltip("狙うObj")] GameObject _targetObject;
    [Range(0F, 90F), Tooltip("発射する角度")] float ThrowingAngle = 45;
    [Tooltip("コントローラーPos")] Vector3 _prevPos = Vector3.zero;
    [Tooltip("差分用List")] List<Vector3> _log = new List<Vector3>();
    [Tooltip("OVRInput.Controller")] OVRInput.Controller rightCont, leftCont, controller;
    [Tooltip("OVRGrabber")] OVRGrabber grab;
    float sabunnkeisann;
    GameObject _throwObj;

    void Start()
    {
        grab = GetComponent<OVRGrabber>();
        rightCont = OVRInput.Controller.RTouch;
        leftCont = OVRInput.Controller.LTouch;
    }

    void Update()
    {
        if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger) || OVRInput.GetUp(OVRInput.RawButton.LHandTrigger))
        {
            if(sabunnkeisann > _sabun)
            {
                ThrowingBall(_throwObj);
                sabunnkeisann = 0;
            }
        }

        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger))//右手
        {
            if (grab.grabbedObject == null){ return; }
            Debug.Log(grab.grabbedObject);
            keisan(rightCont);
            if(grab.grabbedObject.gameObject != null) { _throwObj = grab.grabbedObject.gameObject; }
            else { Debug.Log("null"); }
        }

        if (OVRInput.Get(OVRInput.RawButton.LHandTrigger))//左手
        {
            if (grab.grabbedObject == null) { return; }
            Debug.Log(grab.grabbedObject);
            keisan(leftCont);
            if (grab.grabbedObject.gameObject != null) { _throwObj = grab.grabbedObject.gameObject; }
            else { Debug.Log("null"); }
        }
    }
    void keisan(OVRInput.Controller controller)
    {
        Vector3 current = OVRInput.GetLocalControllerPosition(controller);
        if (_prevPos != Vector3.zero)
        {
            _log.Add(current - _prevPos);
            if (_log.Count > 10) _log.RemoveAt(0);
        }
        _prevPos = current;

        sabunnkeisann = _log.Sum(v => v.magnitude);
    }
    /// <summary>
    /// ボールを射出する
    /// </summary>
    private void ThrowingBall(GameObject obj)
    {
        if (obj != null)
        {
            // Ballオブジェクトの生成
            GameObject ball = Instantiate(obj, this.transform.position, Quaternion.identity);
            // 標的の座標
            Vector3 targetPosition = _targetObject.transform.position;

            // 射出角度
            float angle = ThrowingAngle;

            // 射出速度を算出
            Vector3 velocity = CalculateVelocity(this.transform.position, targetPosition, angle);

            // 射出
            Rigidbody rid = ball.GetComponent<Rigidbody>();
            rid.AddForce(velocity * rid.mass, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 標的に命中する射出速度の計算
    /// </summary>
    /// <param name="pointA">射出開始座標</param>
    /// <param name="pointB">標的の座標</param>
    /// <returns>射出速度</returns>
    private Vector3 CalculateVelocity(Vector3 pointA, Vector3 pointB, float angle)
    {
        // 射出角をラジアンに変換
        float rad = angle * Mathf.PI / 180;

        // 水平方向の距離x
        float x = Vector2.Distance(new Vector2(pointA.x, pointA.z), new Vector2(pointB.x, pointB.z));

        // 垂直方向の距離y
        float y = pointA.y - pointB.y;

        // 斜方投射の公式を初速度について解く
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(x, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (x * Mathf.Tan(rad) + y)));

        if (float.IsNaN(speed))
        {
            // 条件を満たす初速を算出できなければVector3.zeroを返す
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(pointB.x - pointA.x, x * Mathf.Tan(rad), pointB.z - pointA.z).normalized * speed);
        }
        
    }
}
