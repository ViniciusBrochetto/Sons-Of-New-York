using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameMenuController : Photon.PunBehaviour
{

    [Header("Panels")]
    public GameObject pnl_Spawn;
    public GameObject pnl_Options;
    public GameObject pnl_PlayerUI;
    public GameObject pnl_Menu;
    public GameObject pnl_MatchStatus;
    public GameObject pnl_KillInfo;
    public GameObject pnl_EndGame;
    public GameObject pnl_Victory;
    public GameObject pnl_Defeat;

    [Header("Other")]
    public Spawner[] spawnerTeamA;
    public Spawner[] spawnerTeamB;
    public GameObject camera_Spawn;

    private Player currentPlayer;
    private int heroSelected;

    void Update()
    {
        if (PhotonNetwork.room.GetRoomStatus() == RoomStatus.gameEnded)
        {
            HandleEndGameMenus();
        }
        else
        {
            HandleInGameMenus();
        }

    }

    void HandleInGameMenus()
    {

        pnl_KillInfo.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Tab) && !pnl_Menu.GetActive() && !pnl_Options.GetActive())
        {
            pnl_MatchStatus.SetActive(true);

            if (currentPlayer != null)
            {
                pnl_PlayerUI.SetActive(false);
                pnl_KillInfo.SetActive(false);
                currentPlayer.isControllable = false;
            }
            else
                pnl_Spawn.SetActive(false);
        }
        else if (Input.GetKeyUp(KeyCode.Tab) && !pnl_Menu.GetActive() && !pnl_Options.GetActive())
        {
            pnl_MatchStatus.SetActive(false);

            if (currentPlayer != null)
            {
                if (currentPlayer.isAlive)
                {
                    pnl_PlayerUI.SetActive(true);
                    currentPlayer.isControllable = true;
                }
            }
            else
                pnl_Spawn.SetActive(true);
        }
        else if (Input.GetButtonDown("Cancel") && !Input.GetKey(KeyCode.Tab))
        {
            if (pnl_Options.GetActive())
            {
                pnl_Options.SetActive(false);
                pnl_Menu.SetActive(true);
            }
            else if (pnl_Menu.GetActive())
            {
                ResumeMatch();
            }
            else
            {
                if (currentPlayer != null)
                {
                    pnl_PlayerUI.SetActive(false);
                    pnl_KillInfo.SetActive(false);
                    currentPlayer.isControllable = false;
                    Cursor.visible = true;
                }
                Pause();
            }
        }
        else if (currentPlayer != null && !currentPlayer.isAlive)
        {
            pnl_PlayerUI.SetActive(false);
            currentPlayer.isControllable = false;

            if (!pnl_MatchStatus.GetActive() && !pnl_Menu.GetActive() && !pnl_Options.GetActive())
                pnl_KillInfo.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (currentPlayer != null)
        {
            if (pnl_PlayerUI.GetActive() && currentPlayer.isAlive)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Find("txtAmmo").GetComponent<Text>().text = currentPlayer.currWeapon.magazineAmmo.ToString() + "/" + (currentPlayer.currWeapon.unlimitedAmmo ? "---" : currentPlayer.currWeapon.currentAmmo.ToString());
                GameObject.Find("txtLife").GetComponent<Text>().text = "Life: " + currentPlayer.curr_Health;
            }
            else if (!currentPlayer.isAlive && !pnl_Options.GetActive() && !pnl_Menu.GetActive() && !pnl_MatchStatus.GetActive())
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    bool shownEndGame = false;
    bool showingEndGame = false;
    void HandleEndGameMenus()
    {
        if (!showingEndGame)
        {
            showingEndGame = true;
            StartCoroutine(EndGameRoutine());
        }
        else if (shownEndGame)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (pnl_Options.GetActive())
                {
                    pnl_Options.SetActive(false);
                    pnl_Menu.SetActive(true);
                }
                else
                {
                    pnl_Menu.SetActive(!pnl_Menu.GetActive());
                    pnl_MatchStatus.SetActive(!pnl_Menu.GetActive());
                }
            }
        }
    }

    public void Pause()
    {
        if (pnl_Spawn.GetActive())
            pnl_Spawn.SetActive(false);

        pnl_Menu.SetActive(true);
    }

    public void ResumeMatch()
    {

        if (pnl_Options.GetActive())
        {
            pnl_Options.SetActive(false);
            pnl_Menu.SetActive(true);
        }
        else
        {
            pnl_Menu.SetActive(false);

            if (currentPlayer == null)
                pnl_Spawn.SetActive(true);
            else
            {
                if (currentPlayer.isAlive)
                {
                    currentPlayer.isControllable = true;
                    pnl_PlayerUI.SetActive(true);
                }
                else
                {
                    pnl_KillInfo.SetActive(true);
                }
            }
        }
    }

    public void SwitchTeam()
    {
        Team t = PhotonNetwork.player.GetTeam() == Team.TeamA ? Team.TeamB : Team.TeamA;

        if (PunTeams.PlayersPerTeam[t].Count < 4)
            PhotonNetwork.player.SwitchTeam();
    }

    public void ReturnToMenu()
    {
        PhotonNetwork.Disconnect();
        Application.LoadLevel("MainMenu");
    }

    #region EndGame

    public IEnumerator EndGameRoutine()
    {
        pnl_Spawn.SetActive(false);
        pnl_Options.SetActive(false);
        pnl_Menu.SetActive(false);
        pnl_PlayerUI.SetActive(false);
        pnl_MatchStatus.SetActive(false);
        pnl_KillInfo.SetActive(false);

        camera_Spawn.SetActive(true);
        pnl_EndGame.SetActive(true);

        yield return new WaitForSeconds(10f);

        shownEndGame = true;
        pnl_EndGame.SetActive(false);
        pnl_MatchStatus.SetActive(true);

        yield return new WaitForSeconds(10f);

        PhotonNetwork.RemovePlayerCustomProperties(new string[] { PunIsReady.PlayerReadyProp, PunLevelLoaded.PlayerLevelLoadedProp, PunPlayerScores.PlayerKills, PunPlayerScores.PlayerDeaths });

        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.room.SetRoomStatus(RoomStatus.playersLoading);
            photonView.RPC("StartMatch", PhotonTargets.All);
        }
        yield return null;
    }

    public void ObjectiveAccomplished(Team team)
    {
        if (PhotonNetwork.isMasterClient)
            PhotonNetwork.room.SetRoomStatus(RoomStatus.gameEnded);

        GameObject g;
        if (PhotonNetwork.player.GetTeam() == team)
        {
            g = pnl_Victory;
        }
        else
        {
            g = pnl_Defeat;
        }

        if (g != null)
        {
            g.SetActive(true);
        }

    }

    [PunRPC]
    public void StartMatch()
    {
        PhotonNetwork.LoadLevel("Sandbox-Payload");
    }
    #endregion

    #region Spawn

    public void SetHeroSelected(int hero)
    {
        heroSelected = hero;
    }

    public void RequestSpawn()
    {
        PunRequestSpawn(PhotonNetwork.player, heroSelected);
        pnl_Spawn.SetActive(false);
        camera_Spawn.SetActive(false);
        pnl_PlayerUI.SetActive(true);
    }

    //[PunRPC]
    public void PunRequestSpawn(PhotonPlayer player, int heroID)
    {
        Spawner s = null;

        //while (s == null || !s.isFree)
        //{
        if (player.GetTeam() == Team.TeamA)
            s = spawnerTeamA[Random.Range(0, spawnerTeamA.Length)];
        else if (player.GetTeam() == Team.TeamB)
            s = spawnerTeamB[Random.Range(0, spawnerTeamB.Length)];
        //}
        GameObject g;

        g = PhotonNetwork.Instantiate("Player" + heroID, s.transform.position, Quaternion.identity, 0);
        g.name = "LocalPlayer";

        currentPlayer = g.GetComponent<Player>();

        foreach (PhotonView p in g.GetComponents<PhotonView>())
            p.TransferOwnership(player.ID);

        foreach (PhotonView p in g.GetComponentsInChildren<PhotonView>())
            p.TransferOwnership(player.ID);

    }

    public void Respawn()
    {
        pnl_PlayerUI.SetActive(false);
        pnl_KillInfo.SetActive(false);

        if (!pnl_Menu.GetActive() && !pnl_MatchStatus.GetActive())
            pnl_Spawn.SetActive(true);

        camera_Spawn.SetActive(true);
    }

    #endregion
}
