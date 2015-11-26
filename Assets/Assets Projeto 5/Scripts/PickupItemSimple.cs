using UnityEngine;
using System.Collections;

public class PickupItemSimple : Photon.MonoBehaviour
{
    public float SecondsBeforeRespawn = 2;
    public bool pickedUp;
    public bool respawn = true;

    public Transform objectSpawned;
    public WhatToSpawn objectToSpawn;
    public float timeNextSpawn = Mathf.Infinity;

    void Start()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        RespawnAfter();
    }

    void Update()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        if (respawn)
        {
            if (timeNextSpawn < (float)PhotonNetwork.time)
            {
                timeNextSpawn = Mathf.Infinity;
                RespawnAfter();
            }

            if (!pickedUp)
            {
                if (objectSpawned != null)
                {
                    if (Vector3.Distance(this.transform.position, objectSpawned.position) > 5f || objectSpawned.GetComponent<Weapon>().currentAmmo == 0)
                    {
                        pickedUp = true;
                        timeNextSpawn = (float)PhotonNetwork.time + 30f;

                        if (objectSpawned.GetComponent<Weapon>())
                            objectSpawned.GetComponent<Weapon>().requestDestroy = true;
                    }
                }
                else if (objectSpawned == null)
                {
                    pickedUp = true;
                    timeNextSpawn = (float)PhotonNetwork.time + 30f;
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!respawn)
            return;

        if (stream.isWriting)
        {
            if (PhotonNetwork.isMasterClient)
            {
                stream.SendNext(timeNextSpawn);
                stream.SendNext(pickedUp);

                if (objectSpawned != null)
                    stream.SendNext(objectSpawned.GetComponent<PhotonView>().viewID);
                else
                    stream.SendNext(-1);
            }
        }
        else
        {
            timeNextSpawn = (float)stream.ReceiveNext();
            pickedUp = (bool)stream.ReceiveNext();

            int id = (int)stream.ReceiveNext();
            if (id != -1 && PhotonView.Find(id) != null)
                objectSpawned = PhotonView.Find(id).transform;
            else
                objectSpawned = null;
        }
    }

    public void RespawnAfter()
    {
        pickedUp = false;
        objectSpawned = PhotonNetwork.InstantiateSceneObject(objectToSpawn.ToString(), this.transform.position, this.transform.rotation, 0, null).transform;
    }

    public enum WhatToSpawn
    {
        Pistol,
        Shotgun,
        SMG,
        Sniper,
        Bed
    }
}
