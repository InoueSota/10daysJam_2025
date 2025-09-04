using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingScript : MonoBehaviour
{
    [Header("ホーミング関連")]
    [SerializeField] float homingMoveSpeed = 2;
    [SerializeField] float homingRotateSpeed = 180;
    [SerializeField] float angleOffset = 0;

    [SerializeField] string targetName;
    public Transform target;
    // Start is called before the first frame update
    void Awake()
    {
        if (target == null) target = GameObject.Find(targetName).transform;
        if (target == null)
        {
            Debug.LogWarning("targetが見つかりません", gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ターゲットに向かってホーミング
        Vector3 direction = (target.position - transform.position).normalized;
        RotateTowardsDirection2D(transform, homingRotateSpeed, angleOffset);
        transform.position += direction * homingMoveSpeed * Time.deltaTime;
    }

    void RotateTowardsDirection2D(Transform thisTransform, float rotateSpeed, float angleOffset)
    {

        // ターゲットとの方向ベクトルを取得
        Vector2 direction = target.position - thisTransform.position;

        // ベクトルから角度を取得（ラジアン → 度）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;

        // 目標の回転（Quaternion）を計算
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        // 現在の回転から目標回転に向けて徐々に回転
        thisTransform.rotation = Quaternion.RotateTowards(
            thisTransform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}
