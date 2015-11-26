using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HudMatchInfo : MonoBehaviour
{

    [Header("Players")]
    public Text[] playerNamesTeamA;
    public Text[] playerNamesTeamB;
    public Text[] playerPingsTeamA;
    public Text[] playerPingsTeamB;
    public Text[] playerKillsTeamA;
    public Text[] playerKillsTeamB;
    public Text[] playerDeathsTeamA;
    public Text[] playerDeathsTeamB;

    void Update()
    {
        PhotonNetwork.player.SetPing(PhotonNetwork.GetPing());
        if (PhotonNetwork.inRoom)
        {
            int indexA = 0, indexB = 0;
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if (p.GetTeam() == Team.TeamA && indexA < playerNamesTeamA.Length)
                {
                    playerNamesTeamA[indexA].gameObject.SetActive(true);
                    playerNamesTeamA[indexA].text = p.name;

                    playerPingsTeamA[indexA].gameObject.SetActive(true);
                    playerPingsTeamA[indexA].text = p.GetPing().ToString();

                    playerKillsTeamA[indexA].gameObject.SetActive(true);
                    playerKillsTeamA[indexA].text = p.GetKills().ToString();

                    playerDeathsTeamA[indexA].gameObject.SetActive(true);
                    playerDeathsTeamA[indexA].text = p.GetDeaths().ToString();

                    indexA++;
                }
                else if (p.GetTeam() == Team.TeamB && indexB < playerNamesTeamB.Length)
                {
                    playerNamesTeamB[indexB].gameObject.SetActive(true);
                    playerNamesTeamB[indexB].text = p.name;

                    playerPingsTeamB[indexB].gameObject.SetActive(true);
                    playerPingsTeamB[indexB].text = p.GetPing().ToString();

                    playerKillsTeamB[indexB].gameObject.SetActive(true);
                    playerKillsTeamB[indexB].text = p.GetKills().ToString();

                    playerDeathsTeamB[indexB].gameObject.SetActive(true);
                    playerDeathsTeamB[indexB].text = p.GetDeaths().ToString();

                    indexB++;
                }
            }

            for (int i = indexA; i < playerNamesTeamA.Length; i++)
            {
                playerNamesTeamA[i].gameObject.SetActive(false);
                playerPingsTeamA[i].gameObject.SetActive(false);
                playerKillsTeamA[i].gameObject.SetActive(false);
                playerDeathsTeamA[i].gameObject.SetActive(false);
            }

            for (int i = indexB; i < playerNamesTeamB.Length; i++)
            {
                playerNamesTeamB[i].gameObject.SetActive(false);
                playerPingsTeamB[i].gameObject.SetActive(false);
                playerKillsTeamB[i].gameObject.SetActive(false);
                playerDeathsTeamB[i].gameObject.SetActive(false);
            }
        }
    }
}
