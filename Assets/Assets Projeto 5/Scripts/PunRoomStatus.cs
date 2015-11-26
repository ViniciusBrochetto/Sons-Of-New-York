using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunRoomStatus : MonoBehaviour
{
    public const string RoomStatusProp = "currStatus";
    public static RoomStatus roomStatus;

    public void Start()
    {
        roomStatus = RoomStatus.none;
    }

    public void OnJoinedRoom()
    {
        this.UpdateRoomStatus();
    }

    public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        this.UpdateRoomStatus();
    }

    public void UpdateRoomStatus()
    {
        PunRoomStatus.roomStatus = RoomStatus.none;
        RoomStatus roomNewStatus = PhotonNetwork.room.GetRoomStatus();
        roomStatus = roomNewStatus;
    }
}

public enum RoomStatus : byte { none, waitingForPlayers, playersLoading, matchStartCountdown, gameStarted, gameEnded };

static class RoomStatusExtensions
{
    public static void SetRoomStatusAvailable(this Room room, bool enabled)
    {
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("GetRoomStatus was called in outside of a room.");
            return;
        }
        PhotonNetwork.room.SetPropertiesListedInLobby(new string[] { PunRoomStatus.RoomStatusProp});
    }

    public static RoomStatus GetRoomStatus(this Room room)
    {
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("GetRoomStatus was called outside of a room.");
            return RoomStatus.none;
        }

        object roomStatus;
        if (room.customProperties.TryGetValue(PunRoomStatus.RoomStatusProp, out roomStatus))
        {
            return (RoomStatus)roomStatus;
        }

        return RoomStatus.none;
    }

    public static void SetRoomStatus(this Room room, RoomStatus status)
    { 
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("ChangeRoomStatus was called outside of a room.");
            return;
        }

        RoomStatus currRoomStatus = PhotonNetwork.room.GetRoomStatus();
        if (currRoomStatus != status)
        {
            PhotonNetwork.room.SetCustomProperties(new Hashtable() { { PunRoomStatus.RoomStatusProp, (byte)status } });
        }
    }
}