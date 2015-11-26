using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunIsReady : MonoBehaviour
{
    public static Dictionary<PhotonPlayer, bool> PlayersReady;

    public const string PlayerReadyProp = "ready";


    #region Events by Unity and Photon

    public void Start()
    {
        PlayersReady = new Dictionary<PhotonPlayer, bool>();
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
        PlayersReady.Clear();

        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PhotonPlayer player = PhotonNetwork.playerList[i];
            bool playerStatus = player.GetReady();
            PlayersReady.Add(player, playerStatus);
        }
    }
}

static class ReadyExtensions
{
    public static bool GetReady(this PhotonPlayer player)
    {
        object isReady;
        if (player.customProperties.TryGetValue(PunIsReady.PlayerReadyProp, out isReady))
        {
            return (bool)isReady;
        }
        return false;
    }

    public static void SetReady(this PhotonPlayer player, bool isReady)
    {
        if (!PhotonNetwork.connectedAndReady)
        {
            Debug.LogWarning("SetReady was called in state: " + PhotonNetwork.connectionStateDetailed + ". Not connectedAndReady.");
        }

        bool currIsReady = PhotonNetwork.player.GetReady();
        if (currIsReady != isReady)
        {
            PhotonNetwork.player.SetCustomProperties(new Hashtable() { { PunIsReady.PlayerReadyProp, isReady } });
        }
    }
}