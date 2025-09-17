namespace Varwin.XR
{
    /// <summary>
    /// Контроллер телепорта.
    /// </summary>
    public class VarwinXRPlayerMoveTypeTeleport : VarwinXRPlayerMoveBase
    {
        /// <summary>
        /// Локализованное имя пресета.
        /// </summary>
        public override string LocalizationNameKey => "PLAYER_MOVE_TYPE_TELEPORT";

        /// <summary>
        /// При активации включени возможности телепортироваться.
        /// </summary>
        protected override void OnEnable()
        {
            _leftController.InvokeRotate = true;
            _leftController.InvokeTeleport = true;
            _rightController.InvokeTeleport = true;
        }
    }
}