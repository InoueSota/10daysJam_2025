using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingScript : MonoBehaviour
{
    [Header("�z�[�~���O�֘A")]
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
            Debug.LogWarning("target��������܂���", gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // �^�[�Q�b�g�Ɍ������ăz�[�~���O
        Vector3 direction = (target.position - transform.position).normalized;
        RotateTowardsDirection2D(transform, homingRotateSpeed, angleOffset);
        transform.position += direction * homingMoveSpeed * Time.deltaTime;
    }

    void RotateTowardsDirection2D(Transform thisTransform, float rotateSpeed, float angleOffset)
    {

        // �^�[�Q�b�g�Ƃ̕����x�N�g�����擾
        Vector2 direction = target.position - thisTransform.position;

        // �x�N�g������p�x���擾�i���W�A�� �� �x�j
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;

        // �ڕW�̉�]�iQuaternion�j���v�Z
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        // ���݂̉�]����ڕW��]�Ɍ����ď��X�ɉ�]
        thisTransform.rotation = Quaternion.RotateTowards(
            thisTransform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}
