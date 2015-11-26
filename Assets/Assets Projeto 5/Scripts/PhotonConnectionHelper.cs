using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhotonConnectionHelper : Photon.PunBehaviour
{
    [Header("General Variables")]
    public InputField player_Name;

    public InputField server_Ip;
    public InputField server_IpCreateRoom;
    public InputField server_Port;

    public InputField roomName_Create;
    public InputField roomName_Connect;

    public Slider server_MaxPlayers;
    public Text error_Message;
    public Toggle tgl_PhotonCloud;


    [Header("Panels")]
    public GameObject pnl_Lobby;
    public GameObject pnl_ConnectRoom;
    public GameObject pnl_CreateRoom;
    public GameObject pnl_Wait;
    public GameObject pnl_MainMenu;
    public GameObject pnl_ServerConnectionMain;
    private GameObject pnl_ReturnTo;

    private bool creatingRoom = false;


    void Update()
    {
        server_Ip.gameObject.SetActive(!GameOptions.GetUsePhotonCloud());
        server_IpCreateRoom.gameObject.SetActive(!GameOptions.GetUsePhotonCloud());
        server_Port.gameObject.SetActive(!GameOptions.GetUsePhotonCloud());
    }

    public void Connect_ToServer()
    {
        pnl_ReturnTo = pnl_ConnectRoom;
        creatingRoom = false;
        if (GameOptions.GetUsePhotonCloud())
            PhotonNetwork.ConnectToRegion(CloudRegionCode.us, Application.version);
        else
            PhotonNetwork.ConnectToMaster(server_Ip.text, int.Parse(server_Port.text), "3d82be05-0da4-41c6-9df0-6fad50155c27", Application.version);
    }

    public void Connect_ToLocalServer()
    {
        pnl_ReturnTo = pnl_ServerConnectionMain;
        creatingRoom = true;

        if (!GameOptions.GetUsePhotonCloud())
        {
            PhotonNetwork.PhotonServerSettings.ServerAddress = server_IpCreateRoom.text;
            PhotonNetwork.ConnectUsingSettings(Application.version);
        }
        else
        {
            PhotonNetwork.ConnectToRegion(CloudRegionCode.us, Application.version);
        }

        PhotonNetwork.SetMasterClient(PhotonNetwork.player);
    }

    public void Create_Room()
    {
        PhotonNetwork.autoCleanUpPlayerObjects = true;
        pnl_ReturnTo = pnl_CreateRoom;
        pnl_Wait.SetActive(true);
        PhotonNetwork.CreateRoom(roomName_Create.text, new RoomOptions() { maxPlayers = (byte)server_MaxPlayers.value }, null);
    }

    public void Join_RandomRoom()
    {
        pnl_ReturnTo = pnl_ConnectRoom;
        PhotonNetwork.JoinRandomRoom();
    }

    public void Join_Room()
    {
        pnl_Wait.SetActive(true);
        PhotonNetwork.JoinRoom(roomName_Connect.text);
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        error_Message.text = cause.ToString();
        Invoke("ReturnToLastPanel", 3f);
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        error_Message.text = codeAndMsg[1].ToString();
        DisconnectFromServer();
        Invoke("ReturnToLastPanel", 3f);
    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        PhotonNetwork.Disconnect();
        error_Message.text = codeAndMsg[1].ToString();
        Invoke("ReturnToLastPanel", 3f);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.playerName = player_Name.text;

        if (creatingRoom)
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.player);
            Create_Room();
        }
        else
        {
            pnl_Wait.SetActive(false);
            Join_Room();
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.player.customProperties = new ExitGames.Client.Photon.Hashtable();
        if (PhotonNetwork.room.GetRoomStatus() != RoomStatus.waitingForPlayers)
        {
            PhotonNetwork.LoadLevel("Sandbox-Payload");
        }
        else
        {
            pnl_Wait.SetActive(false);
            pnl_Lobby.SetActive(true);
            GameObject.FindObjectOfType<GameLobbyManager>().UpdateServerInfo();
        }
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.room.SetRoomStatusAvailable(true);
        PhotonNetwork.room.SetRoomStatus(RoomStatus.waitingForPlayers);
    }

    void ReturnToLastPanel()
    {
        error_Message.text = "";
        pnl_Wait.SetActive(false);
        pnl_ReturnTo.SetActive(true);
    }
}
