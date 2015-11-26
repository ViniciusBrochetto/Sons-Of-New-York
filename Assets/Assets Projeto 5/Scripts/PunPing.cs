using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunPing : MonoBehaviour
{
    public static Dictionary<PhotonPlayer, int> PlayersPing;

    public const string PlayerPingProp = "ping";


    #region Events by Unity and Photon

    public void Start()
    {
        PlayersPing = new Dictionary<PhotonPlayer, int>();
    }

    public void OnJoinedRoom()
    {
        this.UpdateStatus();
    }

    public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        this.UpdateStatus();
    }

    #endregion


    public void UpdateStatus()
    {
        PlayersPing.Clear();

        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PhotonPlayer player = PhotonNetwork.playerList[i];
            int playerPing = player.GetPing();
            PlayersPing.Add(player, playerPing);
        }
    }
}

static class PingExtensions
{
    public static int GetPing(this PhotonPlayer player)
    {
        object ping;
        if (player.customProperties.TryGetValue(PunPing.PlayerPingProp, out ping))
        {
            return (int)ping;
        }
        return 0;
    }

    public static void SetPing(this PhotonPlayer player, int ping)
    {
        if (!PhotonNetwork.connectedAndReady)
        {
            Debug.LogWarning("SetPing was called in state: " + PhotonNetwork.connectionStateDetailed + ". Not connectedAndReady.");
            return;
        }

        PhotonNetwork.player.SetCustomProperties(new Hashtable() { { PunPing.PlayerPingProp, ping } });
    }
}