using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class PunRoomOptions : MonoBehaviour
{
    public const string RoomFriendlyFireProp = "ffEnabled";
    public const string RoomSingleWeaponProp = "singleWeaponSelection";
    public const string RoomShowNameTagsProp = "ShowNT";
}


static class RoomOptionExtensions
{
    public static void SetFriendlyFire(this Room room, bool enabled)
    {
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("SetFriendlyFire was called outside of a room.");
            return;
        }

        bool currRoomOptions = PhotonNetwork.room.GetFriendlyFire();
        if (currRoomOptions != enabled)
        {
            PhotonNetwork.room.SetCustomProperties(new Hashtable() { { PunRoomOptions.RoomFriendlyFireProp, enabled } });
        }
    }

    public static bool GetFriendlyFire(this Room room)
    {
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("GetFriendlyFire was called outside of a room.");
            return false;
        }

        object roomOption;
        if (room.customProperties.TryGetValue(PunRoomOptions.RoomFriendlyFireProp, out roomOption))
        {
            return (bool)roomOption;
        }

        return false;
    }

    public static void SetShowNameTags(this Room room, bool enabled)
    {
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("SetShowNameTags was called outside of a room.");
            return;
        }

        bool currRoomOptions = PhotonNetwork.room.GetShowNameTags();
        if (currRoomOptions != enabled)
        {
            PhotonNetwork.room.SetCustomProperties(new Hashtable() { { PunRoomOptions.RoomShowNameTagsProp, enabled } });
        }
    }

    public static bool GetShowNameTags(this Room room)
    {
        if (!PhotonNetwork.inRoom)
        {
            Debug.LogWarning("GetShowNameTags was called outside of a room.");
            return true;
        }

        object roomOption;
        if (room.customProperties.TryGetValue(PunRoomOptions.RoomShowNameTagsProp, out roomOption))
        {
            return (bool)roomOption;
        }

        return true;
    }
}