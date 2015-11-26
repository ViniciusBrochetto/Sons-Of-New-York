using UnityEngine;
using System.Collections;

public class CameraShoulderChange : MonoBehaviour
{
    public Transform leftShoulderPos, rightShoulderPosition;
    public Transform weaponPos;
    public float smoothSwitchSpeed = 1f;

    Player playerInfo;

    void Start()
    {
        playerInfo = GetComponentInParent<Player>();
    }

    void LateUpdate()
    {
        Vector3 v = transform.localScale;
        v.x = playerInfo.isWeaponRightShoulder ? 1 : -1;
        transform.localScale = Vector3.Lerp(transform.localScale, v, Time.deltaTime * 5f);
    }
}
