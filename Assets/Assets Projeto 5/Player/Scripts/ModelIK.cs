using UnityEngine;
using System.Collections;

public class ModelIK : MonoBehaviour
{

    public Transform rightHandPos, leftHandPos;
    public Vector3 lookAt;
    public Animator anim;

    void OnAnimatorIK()
    {
        if (leftHandPos != null && rightHandPos != null)
        {
            anim.SetLookAtWeight(1);
            anim.SetLookAtPosition(lookAt);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos.position);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPos.rotation);


            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos.position);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandPos.rotation);
        }

    }
}
