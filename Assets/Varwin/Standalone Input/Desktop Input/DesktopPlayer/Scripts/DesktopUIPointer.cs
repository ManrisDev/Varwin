using UnityEngine;
using Varwin;
using Varwin.DesktopPlayer;
using Varwin.Raycasters;

/// <summary>
/// UI поинтер для Desktop'а.
/// </summary>
public class DesktopUIPointer : DefaultVarwinUIPointer
{
    /// <summary>
    /// Переопределение рейкастера для базовго класса.
    /// </summary>
    protected override IPointableRaycaster Raycaster => _raycaster;

    /// <summary>
    /// Рейкастер.
    /// </summary>
    [SerializeField] private DesktopPlayerRaycaster _raycaster;

    /// <summary>
    /// Можно ли кликнуть по UI объекту.
    /// </summary>
    /// <returns>Можно ли кликнуть.</returns>
    public bool CanClick() => HoveredPointableObject;

    /// <summary>
    /// Обновление состояние поинтера.
    /// </summary>
    public void FixedUpdate()
    {
        if (ProjectData.IsDesktopEditor)
        {
            return;
        }

        UpdateState();
    }
}
