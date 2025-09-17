using System;
using JetBrains.Annotations;
using UnityEngine;
using Varwin.PlatformAdapter;
using Object = UnityEngine.Object;

namespace Varwin
{
    [UsedImplicitly]
    public static class PlayerManager
    {
        public static event Action PlayerRespawned;

        [Obsolete("Не использовать, будет удалено")]
        public static bool GravityFreeze = false;
        public static bool UseGravity = true;
        public static float FallingTime;
        
        [Obsolete("Use MovementEnabled instead")]
        public static bool WasdMovementEnabled = true;

        public static bool MovementTeleportAreaOnly = true;
        public static bool MovementEnabled = true;
        public static bool CursorIsVisible = true;
        public static bool MouseLookEnabled = true;
        public static bool TurnInVREnabled = true; 
        public static float WalkSpeed = 4f;
        public static float CrouchSpeed => WalkSpeed * 0.3f;
        public static float SprintSpeed = 8;
        public static float JumpHeight = 0.5f;
        public static float PlayerNormalHeight = 1.7f;
        public static float PlayerCrouchHeight => PlayerNormalHeight - 0.7f;
        
        public static bool IsInteractable
        {
            get => ProjectData.InteractionWithObjectsLocked;
            set => ProjectData.InteractionWithObjectsLocked = !value;
        }
        
        /// <summary>
        /// Height at which player will be respawned
        /// </summary>
        public static float RespawnHeight = -1000f;

        /// <summary>
        /// The angle of the plane to which the player cannot teleport
        /// </summary>
        public static float TeleportAngleLimit = 45;
        
        public static IPlayerController CurrentRig => InputAdapter.Instance.PlayerController;
        
        public static GameObject Avatar => null;

        public static void TeleportTo(Vector3 position)
        {
            CurrentRig.SetPosition(position);
        }

        public static void SetTeleportEnabled(bool isEnabled)
        {
            TeleportPointer.TeleportEnabled = isEnabled;
        }
        
        public static void SetHideArcOnDisabledTeleport(bool isEnabled)
        {
            TeleportPointer.HideArcOnDisabledTeleport = isEnabled;
        }
        
        [Obsolete("Use MovementEnabled instead")]
        public static void SetDesktopWasdMovementEnabled(bool isEnabled)
        {
            WasdMovementEnabled = isEnabled;
        }

        public static void SetCursorVisible(bool isVisible)
        {
            CursorIsVisible = isVisible;
        }

        public static void SetMouseLookEnabled(bool isEnabled)
        {
            MouseLookEnabled = isEnabled;
        }

        public static void SetTurnInVREnabled(bool isEnabled)
        {
            TurnInVREnabled = isEnabled;
        }
        
        public static void Respawn()
        {
            var anchor = Object.FindObjectOfType<PlayerAnchorManager>();
            if (anchor)
            {
                anchor.SetPlayerPosition();
                PlayerRespawned?.Invoke();
            }
            
            FallingTime = 0;
        }

        public static void Rotate(float angle)
        {
            CurrentRig.Rotation = Quaternion.Euler(CurrentRig.Rotation.eulerAngles + angle * Vector3.up);
        }
        
        public static void RotateTo(Transform targetTransform)
        {
            RotateToTransform(targetTransform);
        }

        private static void RotateToTransform(Transform targetTransform)
        {
            var lookPos = targetTransform.position - CurrentRig.Position;
            lookPos.y = 0;
            CurrentRig.Rotation = Quaternion.LookRotation(lookPos);
        }
        
        public static void SetRotation(Vector3 targetAngle)
        {
            CurrentRig.Rotation = Quaternion.Euler(targetAngle);
        }
    }
}