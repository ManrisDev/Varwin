using UnityEngine;
using UnityEngine.UI;

namespace Varwin.Core.UI.VarwinCanvas
{
    [RequireComponent(typeof(Canvas))]
    public class VarwinCanvas : MonoBehaviour
    {
        [SerializeField]
        private bool workInEditMode;
        
        private void Awake()
        {
            var buttons = GetComponentsInChildren<Button>();

            foreach (var button in buttons)
            {
                VarwinButton varwinButton = button.gameObject.AddComponent<VarwinButton>();
                varwinButton.workInEditMode = workInEditMode;
                
               CreateCollider(button);
            }
        }

        private void CreateCollider(Button button)
        {
            var oldCol = button.GetComponent<Collider>();
            if (oldCol)
            {
                Destroy(oldCol);
            }

            var col = button.gameObject.AddComponent<BoxCollider>();
            var rect = button.GetComponent<RectTransform>().rect;
            col.size = new Vector3(rect.width, rect.height, 0.1f);
        }
    }
}