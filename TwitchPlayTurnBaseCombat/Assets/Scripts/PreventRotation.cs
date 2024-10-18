using UnityEngine;

public class PreventRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // 保存物体的初始旋转
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 每帧将物体的旋转重置为初始旋转
        transform.rotation = initialRotation;
    }

    //hihi
}