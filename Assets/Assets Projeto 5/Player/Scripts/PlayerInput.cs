using UnityEngine;
using System.Collections;
using UnityStandardAssets.Cameras;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

public class PlayerInput : Photon.PunBehaviour
{
    public Player playerInfo;

    public bool offline;

    private float autoPickupTime;
    private bool canTryWeaponPick = true;
    private Weapon recentlyDropped;

    void Start()
    {
        if (photonView.isMine)
            gameObject.GetComponentInChildren<Camera>().enabled = true;
    }

    void Update()
    {
        RaycastHit hit;

        if (!playerInfo.isAlive)
            return;

        #region OwnerOnly
        if (photonView.isMine || offline)
        {
            if (PhotonNetwork.room.GetRoomStatus() == RoomStatus.gameEnded)
            {
                PhotonNetwork.Destroy(this.gameObject);
                return;
            }

            if (playerInfo.curr_Health <= 0 || Input.GetKeyDown(KeyCode.K))
            {
                DropWeapon();
                playerInfo.isAlive = false;
                playerInfo.isControllable = false;
                PhotonNetwork.player.AddDeath(1);

                if (playerInfo.isCarrying)
                    photonView.RPC("PunReleaseBed", PhotonTargets.AllBuffered);

                photonView.RPC("Die", PhotonTargets.AllBuffered);
                Invoke("Destroy", 10f);

                return;
            }

            if (playerInfo.isAlive)
                gameObject.GetComponentInChildren<Camera>().tag = "MainCamera";
            else
                gameObject.GetComponentInChildren<Camera>().tag = "Untagged";

            gameObject.GetComponentInChildren<CameraShoulderChange>().enabled = playerInfo.isControllable && playerInfo.isAlive;
            gameObject.GetComponent<AudioListener>().enabled = playerInfo.isAlive;

            if (!playerInfo.isAlive)
                return;

            #region Movement

            playerInfo.isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0 && playerInfo.isControllable && !playerInfo.isCarrying;

            playerInfo.playerMovement.forwardSpeed = playerInfo.isControllable ? Input.GetAxis("Vertical") : 0f;
            playerInfo.playerMovement.horizontalSpeed = !playerInfo.isSprinting && playerInfo.isControllable ? Input.GetAxis("Horizontal") : 0f;

            #endregion

            if (playerInfo.isControllable)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (playerInfo.bed != null)
                    {
                        if (!playerInfo.isCarrying && playerInfo.bed.GetComponent<PhotonView>().ownerId == 0)
                        {
                            photonView.RPC("PunGrabBed", PhotonTargets.AllBuffered, playerInfo.bed.GetComponent<PhotonView>().viewID);
                        }
                        else if (playerInfo.isCarrying)
                            photonView.RPC("PunReleaseBed", PhotonTargets.AllBuffered);
                    }
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    SwitchShoulder();
                }

                playerInfo.currWeapon.isShooting = Input.GetButton("Fire1") && !playerInfo.isSprinting && (!playerInfo.isCarrying || playerInfo.canShootWhileCarrying);

                if (!playerInfo.isSprinting)
                {
                    if (!playerInfo.isCarrying)
                    {
                        if (Input.GetKeyDown(KeyCode.E) && playerInfo.skillCooldownTimer < (float)PhotonNetwork.time)
                        {
                            if (playerInfo.skill == SpecialSkill.heal)
                            {
                                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 20f, Color.magenta, 0.2f);
                                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20f, 1 << LayerMask.NameToLayer("Player")))
                                {
                                    if (Vector3.Distance(transform.position, hit.point) < 5f)
                                    {
                                        hit.transform.GetComponent<Player>().playerInput.Heal();
                                        playerInfo.skillCooldownTimer = (float)PhotonNetwork.time + playerInfo.skillCooldown;
                                    }
                                }
                            }
                            else if (playerInfo.skill == SpecialSkill.explosives)
                            {
                                GameObject g = PhotonNetwork.Instantiate("Granade", playerInfo.weaponEquippedPositionRight.position + Vector3.up + transform.forward, Quaternion.identity, 0, null);
                                g.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 8f);
                                playerInfo.skillCooldownTimer = (float)PhotonNetwork.time + playerInfo.skillCooldown;
                            }
                            else if (playerInfo.skill == SpecialSkill.xray)
                            {
                                playerInfo.skillCooldownTimer = (float)PhotonNetwork.time + playerInfo.skillCooldown;
                                PhotonView pw;
                                foreach (Player p in GameObject.FindObjectsOfType<Player>())
                                {
                                    pw = p.GetComponent<PhotonView>();
                                    if (pw != null && pw.owner.GetTeam() == PhotonNetwork.player.GetTeam())
                                    {
                                        pw.RPC("PunSeeThroughPower", PhotonTargets.AllBuffered, PhotonNetwork.time + 3f);
                                    }
                                }
                            }
                        }

                        if (playerInfo.currWeapon != null)
                        {
                            if (Input.GetKeyDown(KeyCode.Q))
                            {
                                SwitchWeapon();
                            }
                        }
                    }

                    if (playerInfo.currWeapon != null && (!playerInfo.isCarrying || playerInfo.canShootWhileCarrying))
                    {
                        if (Input.GetKeyDown(KeyCode.R) && !playerInfo.currWeapon.reloading && (playerInfo.currWeapon.magazineAmmo != playerInfo.currWeapon.magazineSize && playerInfo.currWeapon.currentAmmo > 0))
                        {
                            playerInfo.currWeapon.Reload();
                        }
                    }
                }

                if (playerInfo.canShootWhileCarrying || !playerInfo.isCarrying)
                {
                    if (playerInfo.currWeapon.magazineAmmo <= 0 && !playerInfo.currWeapon.reloading)
                    {
                        if (playerInfo.currWeapon.currentAmmo > 0 && !playerInfo.currWeapon.reloading && playerInfo.currWeapon.isEquipped)
                        {
                            playerInfo.currWeapon.Reload();
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) && !playerInfo.isCarrying)
                {
                    playerInfo.playerMovement.Jump();
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    if (playerInfo.specialWeapon != null)
                    {
                        DropWeapon();

                        if (playerInfo.currWeapon == playerInfo.specialWeapon)
                            SwitchWeapon();
                    }
                }

                if (playerInfo.isSprinting)
                {
                    playerInfo.lookCamera.TurnSpeed = Mathf.Lerp(playerInfo.lookCamera.TurnSpeed, GameOptions.GetMouseSensibility() * 0.1f, Time.deltaTime * 5f);
                }
                else
                {
                    if (playerInfo.currWeapon != null && playerInfo.currWeapon.haveZoom && Input.GetButton("Fire2") && playerInfo.isControllable)
                    {
                        gameObject.GetComponentInChildren<Camera>().fieldOfView = Mathf.Lerp(gameObject.GetComponentInChildren<Camera>().fieldOfView, 25, Time.deltaTime * 6f);
                        playerInfo.lookCamera.TurnSpeed = Mathf.Lerp(playerInfo.lookCamera.TurnSpeed, GameOptions.GetMouseSensibility() * 0.5f, Time.deltaTime * 5f);
                    }
                    else
                    {
                        gameObject.GetComponentInChildren<Camera>().fieldOfView = Mathf.Lerp(gameObject.GetComponentInChildren<Camera>().fieldOfView, 60, Time.deltaTime * 6f);
                        playerInfo.lookCamera.TurnSpeed = Mathf.Lerp(playerInfo.lookCamera.TurnSpeed, GameOptions.GetMouseSensibility(), Time.deltaTime * 5f);
                    }
                }
            }

            if (playerInfo.isCarrying)
            {
                if (!playerInfo.canShootWhileCarrying)
                {
                    playerInfo.defaultWeapon.transform.position = playerInfo.weaponPistolHolsterPosition.position;
                    playerInfo.defaultWeapon.transform.rotation = playerInfo.weaponPistolHolsterPosition.rotation;
                }
                if (playerInfo.specialWeapon != null)
                {
                    playerInfo.specialWeapon.transform.position = playerInfo.weaponSpecialHolsterPosition.position;
                    playerInfo.specialWeapon.transform.rotation = playerInfo.weaponSpecialHolsterPosition.rotation;
                }
            }
            else if (playerInfo.specialWeapon != null)
            {
                if (playerInfo.currWeapon == playerInfo.specialWeapon)
                {
                    playerInfo.defaultWeapon.transform.position = playerInfo.weaponPistolHolsterPosition.position;
                    playerInfo.defaultWeapon.transform.rotation = playerInfo.weaponPistolHolsterPosition.rotation;
                }
                else
                {
                    playerInfo.specialWeapon.transform.position = playerInfo.weaponSpecialHolsterPosition.position;
                    playerInfo.specialWeapon.transform.rotation = playerInfo.weaponSpecialHolsterPosition.rotation;
                }
            }

            Vector3 lookAt = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 50f));
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                Vector3 toTarget = (hit.point - transform.position - transform.forward * 1.5f).normalized;

                if (Vector3.Dot(toTarget, transform.forward) > 0)
                    lookAt = hit.point;
            }
            playerInfo.ikControls.lookAt = lookAt;

            if (playerInfo.currWeapon.isEquipped && !playerInfo.isSprinting && (!playerInfo.isCarrying || playerInfo.canShootWhileCarrying))
                playerInfo.currWeapon.LookAtPoint(lookAt);
        }
        #endregion

        PositionWeapons(true);

        SwitchIkPositions();

        if (!photonView.isMine && !offline)
        {
            PhotonPlayer p = PhotonPlayer.Find(photonView.ownerId);
            playerInfo.txtNametag.text = p.name;

            if (p.GetTeam() == PhotonNetwork.player.GetTeam())
            {
                playerInfo.txtNametag.color = Color.green;
                playerInfo.txtNametag.gameObject.SetActive(true);
            }

            if (Camera.main != null)
                playerInfo.txtNametag.transform.LookAt(Camera.main.transform);
        }
    }

    void PositionWeapons(bool lerp)
    {
        if (playerInfo.currWeapon != null && (!playerInfo.isCarrying || playerInfo.canShootWhileCarrying))
        {
            if (playerInfo.isCollidingWithWall)
                playerInfo.currWeapon.Unequip();
            else
                playerInfo.currWeapon.Equip();

            if (!playerInfo.currWeapon.isEquipped || playerInfo.isSprinting)
            {
                playerInfo.currWeapon.transform.position = !lerp ? playerInfo.weaponEquippedDisabledPosition.position : Vector3.Lerp(playerInfo.currWeapon.transform.position, playerInfo.weaponEquippedDisabledPosition.position, 3f * Time.deltaTime);
                playerInfo.currWeapon.transform.rotation = !lerp ? playerInfo.weaponEquippedDisabledPosition.rotation : Quaternion.Lerp(playerInfo.currWeapon.transform.rotation, playerInfo.weaponEquippedDisabledPosition.rotation, 3f * Time.deltaTime);
            }
            else
            {
                playerInfo.currWeapon.transform.position = !lerp ? playerInfo.isWeaponRightShoulder ? playerInfo.weaponEquippedPositionRight.position : playerInfo.weaponEquippedPositionLeft.position : Vector3.Lerp(playerInfo.currWeapon.transform.position, playerInfo.isWeaponRightShoulder ? playerInfo.weaponEquippedPositionRight.position : playerInfo.weaponEquippedPositionLeft.position, 10f * Time.deltaTime);
                playerInfo.currWeapon.Equip();
            }
        }
    }

    void SwitchIkPositions()
    {
        if (playerInfo.isCarrying)
        {
            if (!playerInfo.canShootWhileCarrying)
            {
                playerInfo.ikControls.rightHandPos = playerInfo.bed.FindChild("RightHandGrab");
                playerInfo.ikControls.leftHandPos = playerInfo.bed.FindChild("LeftHandGrab");
            }
            else
            {
                playerInfo.ikControls.rightHandPos = playerInfo.currWeapon.rHandPos1;
                playerInfo.ikControls.leftHandPos = playerInfo.bed.FindChild("LeftHandGrab");
            }
        }
        else if (playerInfo.isWeaponRightShoulder || !playerInfo.currWeapon.isEquipped || playerInfo.isSprinting)
        {
            playerInfo.ikControls.rightHandPos = playerInfo.currWeapon.rHandPos1;
            playerInfo.ikControls.leftHandPos = playerInfo.currWeapon.lHandPos1;
        }
        else
        {
            playerInfo.ikControls.leftHandPos = playerInfo.currWeapon.rHandPos2;
            playerInfo.ikControls.rightHandPos = playerInfo.currWeapon.lHandPos2;
        }
    }

    void SwitchWeapon()
    {
        if (playerInfo.specialWeapon != null && playerInfo.currWeapon == playerInfo.defaultWeapon)
        {
            playerInfo.currWeapon.Holster();
            playerInfo.currWeapon = playerInfo.specialWeapon;
            playerInfo.currWeapon.Equip();
            if (photonView.isMine)
                photonView.RPC("SwitchedWeapon", PhotonTargets.OthersBuffered);
        }
        else if (playerInfo.currWeapon != playerInfo.defaultWeapon)
        {
            playerInfo.currWeapon.Holster();
            playerInfo.currWeapon = playerInfo.defaultWeapon;
            playerInfo.currWeapon.Equip();
            if (photonView.isMine)
                photonView.RPC("SwitchedWeapon", PhotonTargets.OthersBuffered);
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag != "Weapon")
            return;

        PhotonView otherpv = other.transform.GetComponent<PhotonView>();
        if (this.photonView.isMine && otherpv != null && otherpv.ownerId == 0 && !playerInfo.isCarrying)
        {
            if (this.playerInfo.specialWeapon == null && (recentlyDropped != other.transform.GetComponent<Weapon>() || autoPickupTime <= Time.timeSinceLevelLoad) && canTryWeaponPick)
            {
                canTryWeaponPick = false;
                GetWeapon(other.transform.GetComponent<Weapon>());
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Bed")
            return;

        playerInfo.bed = other.transform.parent;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag != "Bed")
            return;

        if (!playerInfo.isCarrying)
            playerInfo.bed = null;
    }

    public void DropWeapon()
    {
        if (playerInfo.specialWeapon != null)
        {
            playerInfo.specialWeapon.photonView.TransferOwnership(0);
            photonView.RPC("WeaponDropped", PhotonTargets.AllBufferedViaServer);
        }
    }

    public void GetWeapon(Weapon w)
    {
        if (playerInfo.specialWeapon == null)
        {
            playerInfo.specialWeapon = w;
            playerInfo.specialWeapon.transform.parent = transform;
            playerInfo.specialWeapon.PlayerGot();

            w.photonView.TransferOwnership(PhotonNetwork.player);
            photonView.RPC("PunGetWeapon", PhotonTargets.AllBuffered, w.photonView.viewID);

            PositionWeapons(false);
        }
    }

    public void Destroy()
    {
        if (photonView.isMine)
        {
            GameObject.FindObjectOfType<GameMenuController>().Respawn();
        }
        PhotonNetwork.Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        if (photonView.isMine)
            PhotonNetwork.RemoveRPCs(photonView);
    }

    public void SwitchShoulder()
    {
        playerInfo.isWeaponRightShoulder = !playerInfo.isWeaponRightShoulder;
    }

    public void Heal()
    {
        photonView.RPC("PunHeal", PhotonTargets.AllBuffered);
    }

    #region RPCs
    [PunRPC]
    public void Die(PhotonMessageInfo message)
    {
        if (photonView.isMine)
        {
            playerInfo.lookCameraDead.enabled = true;
        }

        if (transform.Find("Model").Find("body").GetComponent<Renderer>() != null)
            transform.Find("Model").Find("body").GetComponent<Renderer>().material.mainTexture = playerInfo.deadTex;

        //ragdoll
        playerInfo.animator.enabled = false;
        playerInfo.isAlive = false;
        playerInfo.isControllable = false;
        playerInfo.isCarrying = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }

        foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
        {
            r.interpolation = RigidbodyInterpolation.Interpolate;
            r.maxDepenetrationVelocity = 1.5f;
            r.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            r.isKinematic = false;
            r.useGravity = true;
        }

        foreach (Collider c in GetComponents<Collider>())
        {
            c.enabled = false;
        }

        this.GetComponent<Rigidbody>().isKinematic = true;
        this.GetComponent<Rigidbody>().useGravity = false;

        playerInfo.audioSource.Stop();
        playerInfo.audioSource.volume = 0.5f;

        if (playerInfo.isGrounded)
            playerInfo.audioSource.clip = playerInfo.audio_Death;
        else
            playerInfo.audioSource.clip = playerInfo.audio_Willhelm;

        playerInfo.audioSource.PlayDelayed(0.1f);
    }

    [PunRPC]
    public void SwitchedWeapon()
    {
        SwitchWeapon();
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo message)
    {
        playerInfo.curr_Health -= damage;

        if (playerInfo.curr_Health < 0 && playerInfo.isAlive)
        {
            GameObject.FindObjectOfType<KillInfoManager>().KillTagInfo(message.sender);
        }
    }

    [PunRPC]
    public void PunHeal(PhotonMessageInfo message)
    {
        playerInfo.curr_Health = playerInfo.max_Health;
    }

    [PunRPC]
    public void WeaponDropped()
    {
        autoPickupTime = Time.timeSinceLevelLoad + 2f;
        recentlyDropped = playerInfo.specialWeapon;

        playerInfo.specialWeapon.Drop();
        playerInfo.specialWeapon = null;
    }

    [PunRPC]
    public void PunGetWeapon(int photonViewID, PhotonMessageInfo message)
    {
        if (PhotonNetwork.player == message.sender)
        {
            canTryWeaponPick = true;
            SwitchWeapon();
        }
        else
        {
            playerInfo.specialWeapon = PhotonView.Find(photonViewID).GetComponent<Weapon>();
            playerInfo.specialWeapon.transform.parent = transform;
            playerInfo.specialWeapon.PlayerGot();
        }
    }

    [PunRPC]
    public void PunGrabBed(int bedViewID, PhotonMessageInfo message)
    {
        playerInfo.isCarrying = true;

        if (message.sender.isLocal)
        {
            playerInfo.bed.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player);
            if (playerInfo.currWeapon == playerInfo.specialWeapon && playerInfo.canShootWhileCarrying && message.sender.isLocal)
                SwitchWeapon();
        }
        else
        {
            playerInfo.bed = PhotonView.Find(bedViewID).transform;
        }

        playerInfo.bed.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    [PunRPC]
    public void PunReleaseBed(PhotonMessageInfo message)
    {
        playerInfo.isCarrying = false;

        if (message.sender.isLocal)
        {
            if (playerInfo.currWeapon == playerInfo.defaultWeapon)
                SwitchWeapon();

            playerInfo.bed.GetComponent<PhotonView>().TransferOwnership(0);
        }

        playerInfo.bed.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    [PunRPC]
    public void PunSeeThroughPower(double endTime)
    {
        if (photonView.isMine)
            playerInfo.cameraSeeThrough.SetTimer(endTime);
    }
    #endregion

    #region Server RPC Calls (only server will use)

    public void InformDamage(int damage)
    {
        playerInfo.curr_Health -= damage;
        photonView.RPC("TakeDamage", PhotonTargets.OthersBuffered, damage);
    }

    #endregion

}
