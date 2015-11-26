using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunRoomTimer : MonoBehaviour
{
    public const string RoomEndTimer = "roomEndTime";
}

static class RoomTimerExtensions
{
    public static float GetRoomTime(this Room room)
    {
        object time;
        if (room.customProperties.TryGetValue(PunRoomTimer.RoomEndTimer, out time))
        {
            return (float)time - (float)PhotonNetwork.time;
        }
        return Mathf.Infinity;
    }

    public static string GetRoomTimeInMinutes(this Room room)
    {
        object time;
        if (room.customProperties.TryGetValue(PunRoomTimer.RoomEndTimer, out time))
        {
            string s = "";
            s += (int)(((float)time - (float)PhotonNetwork.time) / 60f);
            s += ":";
            s += (((float)time - (float)PhotonNetwork.time) % 60f).ToString("00");

            return s;
        }
        return "NaN";
    }

    public static void SetRoomGameTime(this Room room, float gameTotalTime)
    {
        if (!PhotonNetwork.connectedAndReady)
        {
            Debug.LogWarning("SetRoomTime was called in state: " + PhotonNetwork.connectionStateDetailed + ". Not connectedAndReady.");
        }

        room.SetCustomProperties(new Hashtable() { { PunRoomTimer.RoomEndTimer, (float)PhotonNetwork.time + gameTotalTime } });
    }
}