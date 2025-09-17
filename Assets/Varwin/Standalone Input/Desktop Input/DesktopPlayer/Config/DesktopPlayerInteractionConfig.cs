using UnityEngine;

namespace Varwin.DesktopPlayer
{
    [CreateAssetMenu(menuName = "Varwin/DesktopPlayer/InteractionConfig")]
    public class DesktopPlayerInteractionConfig : ScriptableObject
    {
        [field: Header("Cursors")]
        [field: SerializeField] public Sprite IdleCursor { get; private set; }
        [field: SerializeField] public Sprite TouchCursor { get; private set; }
        [field: SerializeField] public Sprite UseCursor { get; private set; }
        [field: SerializeField] public Sprite GrabCursor { get; private set; }

        [field: Space, Header("Interaction Properties")]
        [field: SerializeField] public float ObjectRotationSpeed { get; private set; } = 15;
        [field: SerializeField] public float GrabOffset { get; private set; } = 1.3f;
        [field: SerializeField] public float MinGrabDistance { get; private set; } = 0.21f;
        [field: SerializeField] public float MaxGrabDistance { get; private set; } = 1.5f;
        [field: SerializeField] public float ForceGrabCursorDistance { get; private set; } = 1.5f;
    }
}