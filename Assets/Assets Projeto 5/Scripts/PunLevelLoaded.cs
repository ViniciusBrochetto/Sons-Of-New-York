using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunLevelLoaded : MonoBehaviour
{
    public static Dictionary<PhotonPlayer, bool> PlayerLevelLoaded;

    public const string PlayerLevelLoadedProp = "levelLoaded";


    #region Events by Unity and Photon

    public void Start()
    {
        PlayerLevelLoaded = new Dictionary<PhotonPlayer, bool>();
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
        PlayerLevelLoaded.Clear();

        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PhotonPlayer player = PhotonNetwork.playerList[i];
            bool playerStatus = player.GetLevelLoaded();
            PlayerLevelLoaded.Add(player, playerStatus);
        }
    }
}

static class LevelLoadedExtensions
{
    public static bool GetLevelLoaded(this PhotonPlayer player)
    {
        object haveLoaded;
        if (player.customProperties.TryGetValue(PunLevelLoaded.PlayerLevelLoadedProp, out haveLoaded))
        {
            return (bool)haveLoaded;
        }
        return false;
    }

    public static void SetLevelLoaded(this PhotonPlayer player, bool haveLoaded)
    {
        if (!PhotonNetwork.connectedAndReady)
        {
            Debug.LogWarning("SetLoaded was called in state: " + PhotonNetwork.connectionStateDetailed + ". Not connectedAndReady.");
        }

        bool currHaveLoaded = PhotonNetwork.player.GetLevelLoaded();
        if (currHaveLoaded != haveLoaded)
        {
            PhotonNetwork.player.SetCustomProperties(new Hashtable() { { PunLevelLoaded.PlayerLevelLoadedProp, haveLoaded } });
        }
    }
}