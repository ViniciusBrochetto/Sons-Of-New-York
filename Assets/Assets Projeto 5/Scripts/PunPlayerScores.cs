using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class PunPlayerScores : MonoBehaviour
{
    public const string PlayerKills = "kills";
    public const string PlayerDeaths = "deaths";
}


static class ScoreExtensions
{
    public static void SetKills(this PhotonPlayer player, int killNum)
    {
        Hashtable kills = new Hashtable();
        kills[PunPlayerScores.PlayerKills] = killNum;

        player.SetCustomProperties(kills);
    }

    public static void AddKill(this PhotonPlayer player, int killsToAddToCurrent)
    {
        int current = player.GetKills();
        current = current + killsToAddToCurrent;

        Hashtable kills = new Hashtable();
        kills[PunPlayerScores.PlayerKills] = current;

        player.SetCustomProperties(kills);
    }

    public static int GetKills(this PhotonPlayer player)
    {
        object playerKills;
        if (player.customProperties.TryGetValue(PunPlayerScores.PlayerKills, out playerKills))
        {
            return (int)playerKills;
        }

        return 0;
    }

    public static void SetDeaths(this PhotonPlayer player, int newDeathNum)
    {
        Hashtable deaths = new Hashtable();
        deaths[PunPlayerScores.PlayerDeaths] = newDeathNum;

        player.SetCustomProperties(deaths);
    }

    public static void AddDeath(this PhotonPlayer player, int deathsToAddToCurrent)
    {
        int current = player.GetDeaths();
        current = current + deathsToAddToCurrent;

        Hashtable deaths = new Hashtable();  // using PUN's implementation of Hashtable
        deaths[PunPlayerScores.PlayerDeaths] = current;

        player.SetCustomProperties(deaths);  // this locally sets the score and will sync it in-game asap.
    }

    public static int GetDeaths(this PhotonPlayer player)
    {
        object playerDeaths;
        if (player.customProperties.TryGetValue(PunPlayerScores.PlayerDeaths, out playerDeaths))
        {
            return (int)playerDeaths;
        }

        return 0;
    }
}