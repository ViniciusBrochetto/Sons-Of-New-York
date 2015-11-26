using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Cameras
{
    public class FreeLookCam : PivotBasedCameraRig
    {
        public Player playerInfo;

        [Header("Unlocked Cam Vars")]
        [SerializeField]
        private float m_MoveSpeed = 1f;
        [Range(0f, 10f)]
        [SerializeField]
        private float m_TurnSpeed = 1.5f;
        [SerializeField]
        private float m_TiltMax = 75f;
        [SerializeField]
        private float m_TiltMin = 45f;
        [SerializeField]
        private bool m_LockCursor = false;

        private float m_LookAngle;
        private float m_TiltAngle;
        private Vector3 m_PivotEulers;
        private Quaternion m_PivotTargetRot;
        private Quaternion m_TransformTargetRot;

        private Vector3 m_TargetAngles;
        private Vector3 m_FollowAngles;
        private Vector3 m_FollowVelocity;
        private Quaternion m_OriginalRotation;

        [Header("Locked Cam Vars")]
        public float dampingTime = 0.2f;
        public Vector2 rotationRange = new Vector3(70, 70);

        public float TurnSpeed
        {
            get
            {
                return m_TurnSpeed;
            }

            set
            {
                m_TurnSpeed = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            // Lock or unlock the cursor.
            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_LockCursor;
            m_PivotEulers = m_Pivot.rotation.eulerAngles;

            m_PivotTargetRot = m_Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;
        }


        protected void Update()
        {
            if (!GetComponent<PhotonView>().isMine || !playerInfo.isControllable || !playerInfo.isAlive)
                return;

            if (playerInfo.isCarrying)
                HandleRotationLocked();
            else
                HandleRotationMovement();
        }


        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        protected override void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);
        }


        private void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            var x = CrossPlatformInputManager.GetAxis("Mouse X");
            var y = CrossPlatformInputManager.GetAxis("Mouse Y");

            m_LookAngle += x * m_TurnSpeed;
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            m_TiltAngle -= y * m_TurnSpeed;
            m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);

            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_Pivot.localRotation.eulerAngles.y, m_PivotEulers.z);

            m_Pivot.localRotation = m_PivotTargetRot;
            transform.localRotation = m_TransformTargetRot;
        }


        private void HandleRotationLocked()
        {
            m_OriginalRotation = playerInfo.bed.rotation;

            float inputH;
            float inputV;

            inputH = CrossPlatformInputManager.GetAxis("Mouse X");
            inputV = CrossPlatformInputManager.GetAxis("Mouse Y");

            if (m_TargetAngles.y > 180)
            {
                m_TargetAngles.y -= 360;
                m_FollowAngles.y -= 360;
            }
            if (m_TargetAngles.x > 180)
            {
                m_TargetAngles.x -= 360;
                m_FollowAngles.x -= 360;
            }
            if (m_TargetAngles.y < -180)
            {
                m_TargetAngles.y += 360;
                m_FollowAngles.y += 360;
            }
            if (m_TargetAngles.x < -180)
            {
                m_TargetAngles.x += 360;
                m_FollowAngles.x += 360;
            }

            m_TargetAngles.y += inputH * TurnSpeed;
            m_TargetAngles.x += inputV * TurnSpeed;

            m_TargetAngles.y = Mathf.Clamp(m_TargetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f);
            m_TargetAngles.x = Mathf.Clamp(m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);

            m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);
            transform.rotation = m_OriginalRotation * Quaternion.Euler(-m_FollowAngles.x, m_FollowAngles.y, 0);
        }
    }
}
