using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;


[RequireComponent(typeof(PhotonView))]
public class PickupItem : Photon.MonoBehaviour, IPunObservable
{
    public float SecondsBeforeRespawn = 2;
    public bool PickupOnTrigger;

    public bool PickupIsMine;
    public MonoBehaviour OnPickedUpCall;

    public bool SentPickup;
    public double TimeOfRespawn;

    public PickupType type;

    /// <summary></summary>
    public int ViewID { get { return this.photonView.viewID; } }

    public static HashSet<PickupItem> DisabledPickupItems = new HashSet<PickupItem>();


    public void OnTriggerEnter(Collider other)
    {
        PhotonView otherpv = other.GetComponent<PhotonView>();
        if (this.PickupOnTrigger && otherpv != null && otherpv.isMine && other.tag == "Player")
        {
            bool canPickup = false;
            Player p = other.gameObject.GetComponent<Player>();
            switch (type)
            {
                case PickupType.Health:
                    canPickup = p.curr_Health < p.max_Health;
                    break;
                case PickupType.Ammo:
                    if (p.specialWeapon != null)
                        canPickup = p.specialWeapon.currentAmmo + p.specialWeapon.magazineAmmo < p.specialWeapon.startingAmmo + p.specialWeapon.magazineSize;
                    break;
                case PickupType.Vest:
                    canPickup = p.curr_Vest < p.max_Vest;
                    break;
                default:
                    canPickup = false;
                    break;
            }

            if (canPickup)
                this.Pickup(otherpv.viewID);
        }
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting && SecondsBeforeRespawn <= 0)
        {
            stream.SendNext(this.gameObject.transform.position);
        }
        else
        {
            Vector3 lastIncomingPos = (Vector3)stream.ReceiveNext();
            this.gameObject.transform.position = lastIncomingPos;
        }
    }


    public void Pickup(int otherPvID)
    {
        if (this.SentPickup)
        {
            return;
        }

        this.SentPickup = true;
        this.photonView.RPC("PunPickup", PhotonTargets.AllViaServer, otherPvID);
    }


    public void Drop()
    {
        if (this.PickupIsMine)
        {
            this.photonView.RPC("PunRespawn", PhotonTargets.AllViaServer);
        }
    }

    public void Drop(Vector3 newPosition)
    {
        if (this.PickupIsMine)
        {
            this.photonView.RPC("PunRespawn", PhotonTargets.AllViaServer, newPosition);
        }
    }


    [PunRPC]
    public void PunPickup(int otherPvID, PhotonMessageInfo msgInfo)
    {
        if (msgInfo.sender.isLocal) this.SentPickup = false;

        if (!this.gameObject.GetActive())
        {
            Debug.Log("Ignored PU RPC, cause item is inactive. " + this.gameObject + " SecondsBeforeRespawn: " + SecondsBeforeRespawn + " TimeOfRespawn: " + this.TimeOfRespawn + " respawn in future: " + (TimeOfRespawn > PhotonNetwork.time));
            return;
        }

        this.PickupIsMine = msgInfo.sender.isLocal;

        if (this.OnPickedUpCall != null)
        {
            this.OnPickedUpCall.SendMessage("OnPickedUp", this);
        }

        Player p = PhotonView.Find(otherPvID).gameObject.GetComponent<Player>();

        if (p != null)
        {
            switch (type)
            {
                case PickupType.Health:
                    p.curr_Health = p.max_Health;
                    break;
                case PickupType.Ammo:
                    p.specialWeapon.currentAmmo = p.specialWeapon.startingAmmo + (p.specialWeapon.magazineSize - p.specialWeapon.magazineAmmo);
                    break;
                case PickupType.Vest:
                    p.curr_Vest = p.max_Vest;
                    break;
                default:
                    break;
            }
        }

        if (SecondsBeforeRespawn <= 0)
        {
            this.PickedUp(0.0f);
        }
        else
        {
            double timeSinceRpcCall = (PhotonNetwork.time - msgInfo.timestamp);
            double timeUntilRespawn = SecondsBeforeRespawn - timeSinceRpcCall;

            if (timeUntilRespawn > 0)
            {
                this.PickedUp((float)timeUntilRespawn);
            }
        }
    }

    internal void PickedUp(float timeUntilRespawn)
    {
        this.gameObject.SetActive(false);
        PickupItem.DisabledPickupItems.Add(this);
        this.TimeOfRespawn = 0;

        if (timeUntilRespawn > 0)
        {
            this.TimeOfRespawn = PhotonNetwork.time + timeUntilRespawn;
            Invoke("PunRespawn", timeUntilRespawn);
        }
    }


    [PunRPC]
    internal void PunRespawn(Vector3 pos)
    {
        Debug.Log("PunRespawn with Position.");
        this.PunRespawn();
        this.gameObject.transform.position = pos;
    }

    [PunRPC]
    internal void PunRespawn()
    {
#if DEBUG
        double timeDiffToRespawnTime = PhotonNetwork.time - this.TimeOfRespawn;
        if (timeDiffToRespawnTime > 0.1f) Debug.LogWarning("Spawn time is wrong by: " + timeDiffToRespawnTime + " (this is not an error. you just need to be aware of this.)");
#endif

        PickupItem.DisabledPickupItems.Remove(this);
        this.TimeOfRespawn = 0;
        this.PickupIsMine = false;

        if (this.gameObject != null)
        {
            this.gameObject.SetActive(true);
        }
    }


    public enum PickupType
    {
        Health,
        Ammo,
        Vest
    }
}
