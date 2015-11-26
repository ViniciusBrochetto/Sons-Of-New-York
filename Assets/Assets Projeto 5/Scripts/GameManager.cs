using UnityEngine;
using System.Collections;

public class GameManager : Photon.MonoBehaviour
{
    public GameObject pnl_Waiting, pnl_Spawn;
    bool gameStarted = false;

    void Start()
    {
        PhotonNetwork.player.SetLevelLoaded(true);
    }

    void Update()
    {
        if (PhotonNetwork.isMasterClient && PhotonNetwork.room.GetRoomStatus() == RoomStatus.playersLoading)
        {
            bool allLoaded = true;
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if (!p.GetLevelLoaded())
                {
                    allLoaded = false;
                    break;
                }
            }

            if (allLoaded)
            {
                pnl_Waiting.SetActive(false);
                pnl_Spawn.SetActive(true);
                PhotonNetwork.room.SetRoomStatus(RoomStatus.gameStarted);
                PhotonNetwork.room.SetRoomGameTime((60 * 10f) + 10f);
            }
        }

        if (PhotonNetwork.room.GetRoomStatus() == RoomStatus.gameStarted && !gameStarted)
        {
            gameStarted = true;
            pnl_Waiting.SetActive(false);
            pnl_Spawn.SetActive(true);
        }
    }


}
