using UnityEngine;

public class PreventRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // ��������ĳ�ʼ��ת
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // ÿ֡���������ת����Ϊ��ʼ��ת
        transform.rotation = initialRotation;
    }

    //hihi
}