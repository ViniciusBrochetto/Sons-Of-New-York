using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameLobbyManager : Photon.PunBehaviour
{
    [Header("Ready Sprites")]
    public Sprite sprt_IsReady;
    public Sprite sprt_IsNotReady;

    [Header("Server Info")]
    public Text txtPlayersConnected;
    public Toggle tgl_FriendlyFire;
    public Toggle tgl_ShowNT;

    [Header("Players")]
    public Text[] playerNamesTeamA;
    public Text[] playerNamesTeamB;
    public Text[] playerPingsTeamA;
    public Text[] playerPingsTeamB;
    public Image[] playerReadyImagesTeamA;
    public Image[] playerReadyImagesTeamB;

    public List<PhotonPlayer> playersTeamA = new List<PhotonPlayer>();
    public List<PhotonPlayer> playersTeamB = new List<PhotonPlayer>();


    void Start()
    {
        InvokeRepeating("UpdateInfo", 1f, 0.2f);
    }

    void Update()
    {
        if (!PhotonNetwork.inRoom)
            return;

        if (PhotonNetwork.isMasterClient)
        {
            tgl_FriendlyFire.interactable = true;
            tgl_ShowNT.interactable = true;

            if (playersTeamA.Count + playersTeamB.Count >= 1)
            {
                bool allReady = true;

                foreach (PhotonPlayer p in playersTeamA)
                {
                    allReady = p.GetReady();
                    if (!allReady)
                        break;
                }

                if (allReady)
                {
                    foreach (PhotonPlayer p in playersTeamB)
                    {
                        allReady = p.GetReady();
                        if (!allReady)
                            break;
                    }
                }

                if (allReady)
                {
                    PhotonNetwork.automaticallySyncScene = true;
                    PhotonNetwork.room.SetRoomStatus(RoomStatus.playersLoading);
                    photonView.RPC("StartMatch", PhotonTargets.All);
                }
            }
        }

        tgl_FriendlyFire.isOn = PhotonNetwork.room.GetFriendlyFire();
        tgl_ShowNT.isOn = PhotonNetwork.room.GetShowNameTags();

    }

    [PunRPC]
    public void StartMatch()
    {
        PhotonNetwork.LoadLevel("Sandbox-Payload");
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerTeams();
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }
        else
        {
            newPlayer.SetReady(false);

            if (playersTeamA.Count >= playersTeamB.Count)
                newPlayer.SetTeam(Team.TeamA);
            else
                newPlayer.SetTeam(Team.TeamB);
        }
    }

    public void UpdateServerInfo()
    {
        txtPlayersConnected.text = PhotonNetwork.room.playerCount.ToString() + "/" + PhotonNetwork.room.maxPlayers.ToString();
    }

    public void UpdatePlayerInfo()
    {
        int i = 0;
        for (i = 0; i < playerNamesTeamA.Length; i++)
        {
            if (i < playersTeamA.Count)
            {
                playerNamesTeamA[i].text = playersTeamA[i].name;

                playerReadyImagesTeamA[i].gameObject.SetActive(true);
                playerPingsTeamA[i].text = playersTeamA[i].GetPing().ToString();
                playerReadyImagesTeamA[i].sprite = playersTeamA[i].GetReady() ? sprt_IsReady : sprt_IsNotReady;
            }
            else
            {
                playerNamesTeamA[i].text = "";
                playerPingsTeamA[i].text = "";
                playerReadyImagesTeamA[i].gameObject.SetActive(false);
            }

            if (i < playersTeamB.Count)
            {
                playerNamesTeamB[i].text = playersTeamB[i].name;

                playerReadyImagesTeamB[i].gameObject.SetActive(true);
                playerPingsTeamB[i].text = playersTeamB[i].GetPing().ToString();
                playerReadyImagesTeamB[i].sprite = playersTeamB[i].GetReady() ? sprt_IsReady : sprt_IsNotReady;
            }
            else
            {
                playerNamesTeamB[i].text = "";
                playerPingsTeamB[i].text = "";
                playerReadyImagesTeamB[i].gameObject.SetActive(false);
            }
        }
    }

    public void ToggleShowNT()
    {
        PhotonNetwork.room.SetShowNameTags(tgl_ShowNT.isOn);
    }

    public void ToggleFriendlyFire()
    {
        PhotonNetwork.room.SetFriendlyFire(tgl_FriendlyFire.isOn);
    }

    public void ToggleReady()
    {
        PhotonNetwork.player.SetReady(!PhotonNetwork.player.GetReady());
    }

    public void SwitchTeams()
    {
        PhotonNetwork.player.SwitchTeam();
    }

    public void UpdateInfo()
    {
        if (PhotonNetwork.connectedAndReady && PhotonNetwork.inRoom)
        {
            PhotonNetwork.player.SetPing(PhotonNetwork.networkingPeer.RoundTripTime);
            UpdatePlayerInfo();
            UpdatePlayerTeams();
            UpdateServerInfo();
        }
    }

    void UpdatePlayerTeams()
    {
        playersTeamA.Clear();
        playersTeamB.Clear();

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            if (p.GetTeam() == Team.TeamA)
                playersTeamA.Add(p);
            else
                playersTeamB.Add(p);
        }

        UpdatePlayerInfo();
        UpdateServerInfo();
    }
}
