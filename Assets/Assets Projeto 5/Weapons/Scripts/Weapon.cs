using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : Photon.MonoBehaviour
{
    public AudioClip shotSound;
    public AudioClip impactSound;
    public AudioClip reloadSound;

    public int damage;
    public float rps;
    public int magazineSize;
    public int magazineAmmo;
    public int currentAmmo;
    public int startingAmmo;
    public float reloadTime;
    public int bulletsPerShot = 1;

    public bool unlimitedAmmo;
    public bool isAutomatic;
    public bool haveZoom;

    public float maxDispersionRate;
    public float currDispersionRate;
    public float baseDispersionRate;
    public float dispersionDecayPerSecond;
    public float dispersionIncreasePerShot;
    public float dispersionIncreaseMovement;
    public float dispersionIncreaseMovementRate;

    public float maxDamageDistance = 999;
    public float damageLossPerMeter = 0;
    public int minDamage = 0;

    private float nextShotTimer;

    public bool isShooting;
    public bool releasedTrigger;
    public bool isEquipped = false;
    public bool reloading = false;
    public bool isSingleShot = false;

    public Transform barrelEnd;
    public Transform lHandPos1, lHandPos2;
    public Transform rHandPos1, rHandPos2;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    public bool requestDestroy;
    private float destroyTimer = Mathf.Infinity;

    private AudioSource audioSource;
    private Animator animator;

    public ParticleSystem shell;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    public void LateUpdate()
    {
        if (photonView.isMine && isEquipped)
        {
            RaycastHit hit;
            if (isShooting && (releasedTrigger || isAutomatic) && magazineAmmo > 0)
            {
                releasedTrigger = false;
                if (nextShotTimer <= Time.timeSinceLevelLoad)
                {
                    float dispersion = currDispersionRate + currDispersionRate * (dispersionIncreaseMovement * dispersionIncreaseMovementRate);

                    for (int i = 0; i < bulletsPerShot; i++)
                    {

                        Vector3 endPosition = Random.insideUnitSphere * dispersion + barrelEnd.position + barrelEnd.transform.forward * 200f;

                        Physics.Linecast(barrelEnd.position, endPosition, out hit);
                        if (hit.collider != null)
                        {
                            Debug.DrawLine(barrelEnd.position, hit.point, Color.Lerp(Color.green, Color.red, (dispersion - baseDispersionRate) / (maxDispersionRate - baseDispersionRate)), 10f);

                            Shot(barrelEnd.position, hit, PhotonNetwork.player);
                            photonView.RPC("PunHitPointEffects", PhotonTargets.All, hit.point, hit.transform.tag);
                        }
                    }

                    if (animator)
                        animator.SetTrigger("shot");

                    magazineAmmo--;

                    photonView.RPC("PunWeaponShot", PhotonTargets.All);

                    if (currDispersionRate < maxDispersionRate)
                        currDispersionRate += dispersionIncreasePerShot;
                    else
                        currDispersionRate = maxDispersionRate;

                    nextShotTimer = Time.timeSinceLevelLoad + 1f / rps;
                }
            }
            else
            {
                if (currDispersionRate > baseDispersionRate)
                    currDispersionRate -= dispersionDecayPerSecond * Time.deltaTime;
                else
                    currDispersionRate = baseDispersionRate;
            }

            if (!isShooting)
            {
                releasedTrigger = true;
            }
        }

        if (photonView.ownerId == 0)
        {
            if (!unlimitedAmmo && PhotonNetwork.isMasterClient)
            {
                if (destroyTimer == Mathf.Infinity && requestDestroy)
                    destroyTimer = (float)PhotonNetwork.time + 15f;
                else if (destroyTimer < (float)PhotonNetwork.time)
                    PhotonNetwork.Destroy(gameObject);
            }

        }
        else
        {
            destroyTimer = Mathf.Infinity;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(currentAmmo);
            stream.SendNext(magazineAmmo);
            stream.SendNext(destroyTimer);
        }
        else
        {
            currentAmmo = (int)stream.ReceiveNext();
            magazineAmmo = (int)stream.ReceiveNext();
            destroyTimer = (float)stream.ReceiveNext();
        }
    }

    public void Reload()
    {
        reloading = true;
        isShooting = false;

        if (animator)
            animator.SetTrigger("reload");

        Invoke("EndReload", reloadTime);

        if (reloadSound)
        {
            audioSource.clip = reloadSound;
            audioSource.PlayDelayed((ulong)(reloadTime - audioSource.clip.length));
        }

        if (photonView.isMine)
            photonView.RPC("PunReload", PhotonTargets.Others);
    }

    public void EndReload()
    {
        if (magazineAmmo > 0)
        {
            currentAmmo += magazineAmmo;
            magazineAmmo = 0;
        }

        if (!unlimitedAmmo)
        {
            magazineAmmo = Mathf.Min(currentAmmo, magazineSize);

            if (currentAmmo > magazineSize)
                currentAmmo -= magazineSize;
            else
                currentAmmo = 0;
        }
        else
        {
            magazineAmmo = magazineSize;
        }

        if (isSingleShot)
            SpawnShell();

        reloading = false;
    }

    public void Equip()
    {
        isEquipped = true;
    }

    public void Unequip()
    {
        isEquipped = false;
    }

    public void Holster()
    {
        CancelInvoke("EndReload");

        if (audioSource != null)
            audioSource.Stop();

        isEquipped = false;
        isShooting = false;
        reloading = false;
    }

    public void Drop()
    {
        Holster();
        this.transform.parent = null;
        SetColliders(true);

        this.GetComponent<Rigidbody>().useGravity = true;
        this.GetComponent<Rigidbody>().isKinematic = false;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
    }

    public void SetColliders(bool enabled = false)
    {
        foreach (Collider c in this.GetComponents<Collider>())
        {
            c.enabled = enabled;
        }
        foreach (Collider c in this.GetComponentsInChildren<Collider>())
        {
            c.enabled = enabled;
        }
    }

    public void PlayerGot()
    {
        SetColliders(false);
        this.GetComponent<Rigidbody>().useGravity = false;
        this.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void LookAtPoint(Vector3 position)
    {
        if (!reloading)
        {
            Vector3 pos = position - transform.position;
            var newRot = Quaternion.LookRotation(pos);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, 10f * Time.deltaTime);

            //transform.LookAt(position);

            barrelEnd.LookAt(position);
        }
    }

    public void SpawnShell()
    {
        shell.Emit(1);
    }

    public void Shot(Vector3 weaponPosition, RaycastHit hit, PhotonPlayer shooter)
    {
        if (hit.collider.tag == "Player")
        {
            if (hit.collider.GetComponent<Player>())
            {
                if (shooter.GetTeam() != hit.transform.GetComponent<PhotonView>().owner.GetTeam() || PhotonNetwork.room.GetFriendlyFire())
                {
                    //damage calc
                    int calculatedDmg;

                    if (Vector3.Distance(hit.point, this.transform.position) < maxDamageDistance)
                        calculatedDmg = damage;
                    else
                    {
                        float distance = Vector3.Distance(hit.point, this.transform.position);
                        distance -= maxDamageDistance;
                        calculatedDmg = (int)(damage - (damageLossPerMeter * distance));
                        calculatedDmg = Mathf.Max(minDamage, calculatedDmg);
                    }

                    if (hit.transform.GetComponent<Player>().curr_Health > 0 && hit.transform.GetComponent<Player>().curr_Health - calculatedDmg <= 0)
                    {
                        if (shooter.GetTeam() == hit.transform.GetComponent<PhotonView>().owner.GetTeam())
                            shooter.AddKill(-1);
                        else
                            shooter.AddKill(1);
                    }

                    hit.transform.GetComponent<PlayerInput>().InformDamage(calculatedDmg);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (photonView.isMine)
            PhotonNetwork.RemoveRPCs(photonView);
    }

    public void OnOwnershipRequest(object[] viewAndPlayer)
    {
        PhotonView view = viewAndPlayer[0] as PhotonView;
        PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;

        Debug.Log("OnOwnershipRequest(): Player " + requestingPlayer + " requests ownership of: " + view + ".");

        if (PhotonNetwork.isMasterClient)
        {
            if (this.photonView.ownerId == 0)
                view.TransferOwnership(requestingPlayer.ID);
        }
    }

    [PunRPC]
    public void PunReload()
    {
        Reload();
    }

    [PunRPC]
    public void PunHitPointEffects(Vector3 hitPoint, string hitTag)
    {
        if (hitTag == "Player")
            GameObject.Instantiate(Resources.Load("particle_HitPlayer"), hitPoint, Quaternion.identity);
        else
            GameObject.Instantiate(Resources.Load("particle_HitWall"), hitPoint, Quaternion.identity);

        AudioSource.PlayClipAtPoint(impactSound, hitPoint, 1f);
    }

    [PunRPC]
    public void PunWeaponShot()
    {
        if (!isSingleShot)
            SpawnShell();

        AudioSource.PlayClipAtPoint(shotSound, this.transform.position, 0.5f);
    }
}

