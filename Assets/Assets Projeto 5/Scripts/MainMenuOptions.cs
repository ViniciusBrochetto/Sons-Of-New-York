using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuOptions : MonoBehaviour {

    public InputField playerName;
    public Slider mouseSensibility;
    public Toggle usePhotonCloud;
    public Toggle equipWeaponOnPickup;

    public void Awake()
    {
        LoadOptions();
    }

    public void LoadOptions()
    {
        playerName.text = GameOptions.GetPlayerName();
        mouseSensibility.value = GameOptions.GetMouseSensibility();
        usePhotonCloud.isOn = GameOptions.GetUsePhotonCloud();
        equipWeaponOnPickup.isOn = GameOptions.GetEquipOnPickup();
    }

    public void ApplyOptions()
    {
        GameOptions.SetMouseSensibility(mouseSensibility.value);
        GameOptions.SetPlayerName(playerName.text);
        GameOptions.SetPhotonCloud(usePhotonCloud.isOn);
        GameOptions.SetEquipOnPickup(equipWeaponOnPickup.isOn);


        if (PhotonNetwork.connected)
            PhotonNetwork.player.name = GameOptions.GetPlayerName();
    }
}
