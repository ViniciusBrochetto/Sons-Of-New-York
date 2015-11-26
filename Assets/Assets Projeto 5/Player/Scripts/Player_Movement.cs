using UnityEngine;
using System.Collections;

public class Player_Movement : Photon.MonoBehaviour
{
    public Player playerInfo;

    public float horizontalSpeed, forwardSpeed;

    public AudioClip[] steps;

    void FixedUpdate()
    {
        if (photonView.isMine)
        {
            if (horizontalSpeed != 0f || forwardSpeed != 0f)
            {
                //remover
                if (Input.GetKey(KeyCode.K))
                    playerInfo.curr_Health = 0;

                if (!playerInfo.isAlive)
                    return;

                if (forwardSpeed < 0)
                    forwardSpeed = Mathf.Max(forwardSpeed, -0.5f);


                if (playerInfo.isCarrying)
                {
                    Vector3 v = playerInfo.bed.position;
                    v += playerInfo.bed.forward * playerInfo.speed * 0.5f * Time.deltaTime * forwardSpeed;
                    playerInfo.bed.GetComponent<Rigidbody>().MovePosition(v);

                    float f = playerInfo.speed * 6f * Time.deltaTime * horizontalSpeed * forwardSpeed;
                    playerInfo.bed.Rotate(playerInfo.bed.up, f);
                }
                else
                {
                    Vector3 v = this.transform.position;
                    if (!playerInfo.isCollidingWithWall || forwardSpeed < 0)
                        v += transform.forward * playerInfo.speed * Time.deltaTime * forwardSpeed * (playerInfo.isSprinting ? playerInfo.sprint_multiplier : 1f);
                    if ((!playerInfo.isBedColliding))
                        v += transform.right * playerInfo.speed * 0.70f * Time.deltaTime * horizontalSpeed;

                    this.GetComponent<Rigidbody>().MovePosition(v);
                }
            }

            if (forwardSpeed != 0 || horizontalSpeed != 0)
                playerInfo.isMoving = true;
            else
                playerInfo.isMoving = false;

            if (playerInfo.isCarrying)
            {
                this.transform.position = playerInfo.bed.position;
                this.transform.rotation = playerInfo.bed.rotation;
            }

            Vector2 mov = Vector2.zero;
            mov.x = forwardSpeed * (playerInfo.isSprinting ? 2f : 1f) * (playerInfo.isCarrying ? 0.5f : 1f);
            mov.y = horizontalSpeed * (playerInfo.isCarrying ? 0.5f : 1f);
            mov.y *= (playerInfo.isCarrying && mov.x == 0 ? 0f : 1f);

            playerInfo.animator.SetFloat("Forward", mov.x);
            playerInfo.animator.SetFloat("Sideway", mov.y);

            RaycastHit ray;
            playerInfo.isGrounded = Physics.Raycast(this.transform.position + Vector3.up, Vector3.down, out ray, 1.3f);
            playerInfo.animator.SetBool("grounded", playerInfo.isGrounded);

            Debug.DrawLine(this.transform.position, ray.point);
        }
    }

    void Update()
    {
        if (photonView.isMine && playerInfo.currWeapon != null)
        {
            if (!playerInfo.isGrounded)
                playerInfo.currWeapon.dispersionIncreaseMovement = 1f;
            else if (playerInfo.isMoving)
                playerInfo.currWeapon.dispersionIncreaseMovement = 0.5f;
            else
                playerInfo.currWeapon.dispersionIncreaseMovement = 0f;
        }
    }

    public void Jump()
    {
        if (playerInfo.isGrounded)
        {
            playerInfo.animator.SetTrigger("jump");
            this.GetComponent<Rigidbody>().AddForce(Vector3.up * playerInfo.jumpSpeed, ForceMode.VelocityChange);
        }
    }

    float timeStep = 0f;
    public void Step()
    {
        if (timeStep < Time.timeSinceLevelLoad)
        {
            timeStep = Time.timeSinceLevelLoad + 0.2f;
            AudioSource.PlayClipAtPoint(steps[Random.Range(0, steps.Length)], this.transform.position, 1f);
        }
    }
}
