using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KillInfoManager : MonoBehaviour {

    public Image img_Charachter, img_Weapon;
    public Text lbl_PlayerName;

    public void KillTagInfo(PhotonPlayer player)
    {
        lbl_PlayerName.text = player.name;
    }

}
