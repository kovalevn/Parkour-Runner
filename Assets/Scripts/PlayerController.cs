using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerState { Running, Jumping, TempoJumping, Airborne, Falling, Landing, Turning, Dead }
public enum PlayerAirborneState { Regular, Flipping, Spinning }

public class PlayerController : MonoBehaviour
{
    //Переменные компонентов
    public Animator anim;
    public Rigidbody rb;
    public Transform mainObj;
    public Transform bodyPos;
    private ScoreCounter scoreCounter;
    private CameraHandler cameraHandler;

    //Переменные, отвечающие за текущее состояние игрока
    public PlayerState playerState = PlayerState.Running;
    public PlayerAirborneState playerAirborneState = PlayerAirborneState.Regular;

    public bool goodLand;
    public bool tempo;
    public bool rotationSpeedUp;
    public bool stillSpinning;

    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;
    private Camera mainCam;
    public Transform airPosition;
    public Transform capsule;
    public Transform rootx;

    public Vector3 distToMainObj;

    private float mainColliderHeight;
    float distToGround;
    float distToWall;

    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    public float degreesToRotate;

    private void Awake()
    {
        ragdollColliders = GetComponentsInChildren<Collider>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        SwitchRagdollDead(false);
    }

    // Start is called before the first frame update
    private void Start()
    {
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
        distToMainObj = mainObj.position - mainCam.transform.position;
        mainColliderHeight = mainObj.GetComponent<CapsuleCollider>().height;
        distToGround = mainObj.GetComponent<CapsuleCollider>().bounds.extents.y;
        distToWall = mainObj.GetComponent<CapsuleCollider>().bounds.extents.z;
        target = mainObj.transform;
        scoreCounter = FindObjectOfType<ScoreCounter>();
        cameraHandler = FindObjectOfType<CameraHandler>();
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;

        //То, что происходит, когда игрок умер
        if (playerState == PlayerState.Dead)
        {
            cameraHandler.FollowTarget(bodyPos, delta);
            scoreCounter.ShowFinalResult();
            if (Input.GetKeyDown(KeyCode.Space)) Restart();
        }
        //То, что происходит, когда игрок жив
        else 
        {
            //То, что происходит в любой PlayerState, кроме смерти
            cameraHandler.FollowTarget(target, delta);

            if (Input.GetKeyDown(KeyCode.LeftControl)) SwitchRagdollDead(true);

            //То, что  происходит в любой PlayerState, кроме указанных
            if (playerAirborneState != PlayerAirborneState.Flipping) Tuck(false);

            //То, что происодит, когда игрок в определённом PlayerState
            switch (playerState)
            {
                case PlayerState.Running:
                    MoveForward();
                    if (!GroundCheck())
                    {
                        anim.SetBool("Falling", true);
                        playerState = PlayerState.Falling;
                    }
                    else anim.SetBool("Falling", false);
                    if (WallHitCheck())
                    {
                        SwitchRagdollDead(true);
                    }

                    //Ввод игрока
                    if (GetJumpKey())
                    {
                        Flip();
                    }
                    break;

                case PlayerState.Jumping:
                    MoveForward();
                    if (WallHitCheck())
                    {
                        SwitchRagdollDead(true);
                    }
                    break;

                case PlayerState.Airborne:
                    Rotate(2);
                    if ((!GetSpinKey() || (GetSpinKey() && Input.GetKey(KeyCode.Space))) && ((mainObj.transform.localRotation.eulerAngles.y > 10 && mainObj.transform.localRotation.eulerAngles.y < 170)
                        || (mainObj.transform.localRotation.eulerAngles.y > 190 && mainObj.transform.localRotation.eulerAngles.y < 350)))
                    {
                        stillSpinning = true;
                        Spin(12);
                    }
                    else stillSpinning = false;
                    scoreCounter.CountFlips(mainObj);

                    //Ввод игрока
                    if (GetJumpKey() && !GetSpinKey())
                    {
                        playerAirborneState = PlayerAirborneState.Flipping;
                    }
                    else if (GetSpinKey() && !GetJumpKey())
                    {
                        playerAirborneState = PlayerAirborneState.Spinning;
                    }
                    else
                    {
                        playerAirborneState = PlayerAirborneState.Regular;
                    }

                    switch (playerAirborneState)
                    {
                        case PlayerAirborneState.Flipping:
                            if (rotationSpeedUp) Rotate(tempo ? 10 : 9);
                            anim.SetBool("Spin", false);
                            anim.SetBool("Tuck", true);
                            Tuck(true);
                            if (!GetJumpKey() || GetSpinKey())
                            {
                                anim.SetBool("Tuck", false);
                                playerAirborneState = PlayerAirborneState.Regular;
                            }
                            break;

                        case PlayerAirborneState.Spinning:
                            if (rotationSpeedUp) Rotate(tempo ? 5 : 4);
                            Spin(12);
                            anim.SetBool("Tuck", false);
                            anim.SetBool("Spin", true);
                            if (!GetSpinKey() || GetJumpKey())
                            {
                                anim.SetBool("Spin", false);
                                playerAirborneState = PlayerAirborneState.Regular;
                            }
                            break;

                        case PlayerAirborneState.Regular:
                            anim.SetBool("Spin", false);
                            anim.SetBool("Tuck", false);
                            break;
                    }
                    break;

                case PlayerState.Falling:
                    MoveForward();
                    break;

                case PlayerState.Landing:
                    tempo = false;
                    CheckLanding();

                    //Ввод игрока
                    if (GetJumpKey() && playerState != PlayerState.Dead) TempoFlip();
                    break;
            }
        }
    }

    #region Air rotation methods

    public void Rotate(float speed)
    {
        mainObj.RotateAround(airPosition.position, Vector3.right, speed);
    }

    public void Spin(float speed)
    {
        mainObj.transform.Rotate(Vector3.up, speed);
    }

    #endregion

    #region Ragdoll methods

    public void SwitchRagdollDead(bool condition)
    {
        if (ragdollColliders.Length < 1) return;

        if (anim != null) anim.enabled = !condition;
        rb.transform.GetComponent<Collider>().isTrigger = condition;
        rb.useGravity = !condition;
        if (condition) 
        {
            playerState = PlayerState.Dead;
            scoreCounter.AddResultDead();
        }

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.useGravity = condition;
            rb.isKinematic = !condition;
            if (condition) rb.velocity = this.rb.velocity;
        }

        if (condition)
        {
            rb.velocity = Vector3.zero;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.isTrigger = !condition;
        }
    }

    #endregion

    #region Jumps methods

    public void JumpUp()
    {
        rb.AddForce(new Vector3(0, 0.7f, 0.3f) * 150, ForceMode.Impulse);
        goodLand = false;
        if (playerState == PlayerState.TempoJumping) scoreCounter.CountCombo();
        anim.SetBool("TempoFront", false);
        playerState = PlayerState.Airborne;
    }

    private void Flip()
    {
        anim.SetTrigger("JumpFront");
        playerState = PlayerState.Jumping;
    }

    private void TempoFlip()
    {
        anim.SetBool("TempoFront", true);
        tempo = true;
        playerState = PlayerState.TempoJumping;
    }

    #endregion

    #region Player input checks

    public bool GetJumpKey()
    {
        return Input.GetKey(KeyCode.Space) || Input.touchCount == 1;
    }

    public bool GetSpinKey()
    {
        return Input.GetKey(KeyCode.LeftArrow) || Input.touchCount > 1;
    }

    #endregion

    #region Landing methods

    private void CheckLanding()
    {
        //Если игрок не в группировке, не в процессе бокового вращения, на земле, и в достаточно вертикальном положении, то преземление успешно
        if (playerAirborneState == PlayerAirborneState.Regular && !stillSpinning && Mathf.Abs(CheckRotation.ColliderVerticalAngle(mainObj)) < 30
            && mainObj.GetComponent<CapsuleCollider>().height > 1.2f && GroundCheck())
        {
            if (!goodLand)
            {
                if (CheckRotation.FacingForward(mainObj)) anim.SetTrigger("Landed");
                if (CheckRotation.FacingBackward(mainObj)) anim.SetTrigger("LandedBack");
                goodLand = true;
            }

            if (CheckRotation.ColliderVerticalAngle(mainObj) > 5 &&
                mainObj.transform.rotation.eulerAngles.x < 379 && mainObj.transform.rotation.eulerAngles.x > 30)
            {
                if (mainObj.transform.rotation.eulerAngles.y > 0) Land(-1);
                else Land(1);
            }
            else if (CheckRotation.ColliderVerticalAngle(mainObj) > 5 &&
                mainObj.transform.rotation.eulerAngles.x > 0 && mainObj.transform.rotation.eulerAngles.x < 30)
            {
                if (mainObj.transform.rotation.eulerAngles.y > 0) Land(1);
                else Land(-1);
            }
        }
        else
        {
            if (!goodLand) SwitchRagdollDead(true);
        }
    }

    public void Land(int side)
    {
        mainObj.RotateAround(airPosition.position, Vector3.right * side, 5);
    }

    private bool GroundCheck()
    {
        return Physics.Raycast(mainObj.transform.position, -Vector3.up, distToGround + 0.1f);
    }

    #endregion

    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }

    private bool WallHitCheck()
    {
        var layerMask = ~(1 << 8);
        return Physics.Raycast(mainObj.transform.position, Vector3.forward, distToWall + 0.2f, layerMask);
    }

    public void ResetAvatarRotation()
    {
        transform.rotation = Quaternion.identity;
    }

    public void StartRun()
    {
        if (playerState == PlayerState.TempoJumping || playerState == PlayerState.Jumping) return;
        goodLand = false;
        tempo = false;
        scoreCounter.AddResult();
        playerState = PlayerState.Running;
    }

    private void MoveForward()
    {
        mainObj.position += (new Vector3(0, 0, 1) * 4 * Time.deltaTime);
        ResetAvatarRotation();
    }

    public void Tuck(bool condition)
    {
        if (mainObj.GetComponent<CapsuleCollider>().height > mainColliderHeight / 2 && condition)
            mainObj.GetComponent<CapsuleCollider>().height = Mathf.Lerp(mainObj.GetComponent<CapsuleCollider>().height, mainColliderHeight / 2, 0.3f);
        else if (mainObj.GetComponent<CapsuleCollider>().height < mainColliderHeight && !condition)
            mainObj.GetComponent<CapsuleCollider>().height = Mathf.Lerp(mainObj.GetComponent<CapsuleCollider>().height, mainColliderHeight, 0.3f);
    }

    public void RotationSpeedUp()
    {
        rotationSpeedUp = true;
    }

    public void RotationSlowDown()
    {
        rotationSpeedUp = false;
    }

    public void Turn()
    {
        mainObj.rotation = Quaternion.Euler(0,0,0);
    }

    public void IsTurning()
    {
        playerState = PlayerState.Turning;
    }
}
