using UnityEngine;
using System.Collections;

public static class GameOptions {

    public static string OPT_PLAYER_NAME = "opt_player_name";
    public static string OPT_MOUSE_SENSIBILITY = "opt_mouse_sensibility";
    public static string OPT_MOUSE_ZOOM_SENSIBILITY = "opt_mouse_zoom_sensibility";
    public static string OPT_USE_PHOTONCLOUD = "opt_use_photoncloud";
    public static string OPT_EQUIP_ON_PICKUP = "opt_equip_on_pickup";

    public static void SetPlayerName(string n)
    {
        PlayerPrefs.SetString(OPT_PLAYER_NAME, n);
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(OPT_PLAYER_NAME, "Default Name");
    }



    public static void SetMouseSensibility(float s)
    {
        PlayerPrefs.SetFloat(OPT_MOUSE_SENSIBILITY, s);
    }

    public static float GetMouseSensibility()
    {
        return PlayerPrefs.GetFloat(OPT_MOUSE_SENSIBILITY, 1f);
    }


    public static void SetMouseZoomSensibility(float s)
    {
        PlayerPrefs.SetFloat(OPT_MOUSE_ZOOM_SENSIBILITY, s);
    }

    public static float GetMouseZoomSensibility()
    {
        return PlayerPrefs.GetFloat(OPT_MOUSE_ZOOM_SENSIBILITY, 1f);
    }


    public static void SetPhotonCloud(bool b)
    {
        PlayerPrefs.SetInt(OPT_USE_PHOTONCLOUD, b ? 1 : 0);
    }

    public static bool GetUsePhotonCloud()
    {
        return PlayerPrefs.GetInt(OPT_USE_PHOTONCLOUD, 0) == 1 ? true : false;
    }


    public static void SetEquipOnPickup(bool b)
    {
        PlayerPrefs.SetInt(OPT_EQUIP_ON_PICKUP, b ? 1 : 0);
    }

    public static bool GetEquipOnPickup()
    {
        return PlayerPrefs.GetInt(OPT_EQUIP_ON_PICKUP, 0) == 1 ? true : false;
    }
}
