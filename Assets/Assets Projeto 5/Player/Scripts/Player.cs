using UnityEngine;
using System.Collections;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.Utility;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Status")]
    public float curr_Health = 100f;
    public float max_Health = 100f;
    public float curr_Vest = 0f;
    public float max_Vest = 100f;
    public float speed;
    public float sprint_multiplier;
    public float carrying_multiplier;
    public float jumpSpeed;


    [Header("Skill")]
    public SpecialSkill skill;
    public float skillCooldown;
    public float skillCooldownTimer;


    [Header("Flags")]
    public bool isGrounded;
    public bool isCrouched;
    public bool isAlive;
    public bool isMoving;
    public bool isSprinting;
    public bool isCollidingWithWall;
    public bool isBedColliding;
    public bool isCarrying;
    public bool isControllable;
    public bool canShootWhileCarrying;
    public bool isWeaponRightShoulder;


    [Header("Weapons")]
    public Weapon currWeapon;
    public Weapon defaultWeapon;
    public Weapon specialWeapon;

    [Header("Scripts")]
    public PlayerInput playerInput;
    public CameraShoulderChange shoulderCameraControl;
    public seeThroughCameraControl cameraSeeThrough;
    public Player_Movement playerMovement;
    public FreeLookCam lookCamera;
    public SimpleMouseRotator2 lookCameraDead;
    public Animator animator;
    public ModelIK ikControls;
    public Text txtNametag;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip audio_Willhelm;
    public AudioClip audio_Death;
    public AudioClip audio_Pain;

    [Header("Positions")]
    public Transform weaponSpecialHolsterPosition;
    public Transform weaponPistolHolsterPosition;
    public Transform weaponEquippedPositionRight;
    public Transform weaponEquippedPositionLeft;
    public Transform weaponEquippedDisabledPosition;
    public Transform bed;

    [Header("Materials")]
    public Texture aliveTex;
    public Texture deadTex;
}


public enum SpecialSkill
{
    none,
    explosives,
    xray,
    heal
}