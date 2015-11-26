using UnityEngine;
using System.Collections;

public class PlayerColisionInfo : MonoBehaviour
{

    Player playerInfo;

    void Start()
    {
        playerInfo = GetComponentInParent<Player>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (this.tag == "WeaponBlockTest")
            playerInfo.isCollidingWithWall = true;
        else if (this.tag == "BedBlockTest")
            playerInfo.isBedColliding = true && playerInfo.isCarrying;
    }

    void OnTriggerExit(Collider other)
    {
        if (this.tag == "WeaponBlockTest")
            playerInfo.isCollidingWithWall = false;
        else if (this.tag == "BedBlockTest")
            playerInfo.isBedColliding = false;
    }
}
