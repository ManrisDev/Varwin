using UnityEngine;
using UnityEngine.UI;
using Varwin.PlatformAdapter;

public struct ObjectTooltipEventArgs
{
    public string newText;
}

public delegate void ObjectTooltipEventHandler(object sender, ObjectTooltipEventArgs e);

public class TooltipObject : MonoBehaviour
    {
        [Tooltip("The text that is displayed on the tooltip.")]
        public string displayText;
        [Tooltip("The size of the text that is displayed.")]
        public int fontSize = 14;
        [Tooltip("The size of the tooltip container where `x = width` and `y = height`.")]
        public Vector2 containerSize = new Vector2(0.1f, 0.03f);
        [Tooltip("An optional transform of where to start drawing the line from. If one is not provided the centre of the tooltip is used for the initial line position.")]
        public Transform drawLineFrom;
        [Tooltip("A transform of another object in the scene that a line will be drawn from the tooltip to, this helps denote what the tooltip is in relation to. If no transform is provided and the tooltip is a child of another object, then the parent object's transform will be used as this destination position.")]
        public Transform drawLineTo;
        [Tooltip("The width of the line drawn between the tooltip and the destination transform.")]
        public float lineWidth = 0.001f;
        [Tooltip("The colour to use for the text on the tooltip.")]
        public Color fontColor = Color.black;
        [Tooltip("The colour to use for the background container of the tooltip.")]
        public Color containerColor = Color.black;
        [Tooltip("The colour to use for the line drawn between the tooltip and the destination transform.")]
        public Color lineColor = Color.black;
        [Tooltip("If this is checked then the tooltip will be rotated so it always face the headset.")]
        public bool alwaysFaceHeadset = false;

        public Material materialForTooltipText;
        public Text tooltipText;
        public Text tooltipReverseText;

        /// <summary>
        /// Emitted when the object tooltip is reset.
        /// </summary>
        public event ObjectTooltipEventHandler ObjectTooltipReset;
        /// <summary>
        /// Emitted when the object tooltip text is updated.
        /// </summary>
        public event ObjectTooltipEventHandler ObjectTooltipTextUpdated;

        protected LineRenderer line;
        protected Transform headset;

        public virtual void OnObjectTooltipReset(ObjectTooltipEventArgs e)
        {
            ObjectTooltipReset?.Invoke(this, e);
        }

        public virtual void OnObjectTooltipTextUpdated(ObjectTooltipEventArgs e)
        {
            ObjectTooltipTextUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// The ResetTooltip method resets the tooltip back to its initial state.
        /// </summary>
        public virtual void ResetTooltip()
        {
            SetContainer();
            SetText("UITextFront");
            SetText("UITextReverse");
            SetLine();
            if (!drawLineTo && transform.parent)
            {
                drawLineTo = transform.parent;
            }
            OnObjectTooltipReset(SetEventPayload());

            if (tooltipText && tooltipReverseText && materialForTooltipText)
            {
                tooltipText.material = materialForTooltipText;
                tooltipReverseText.material = materialForTooltipText;
            }
        }

        /// <summary>
        /// The UpdateText method allows the tooltip text to be updated at runtime.
        /// </summary>
        /// <param name="newText">A string containing the text to update the tooltip to display.</param>
        public virtual void UpdateText(string newText)
        {
            displayText = newText;
            OnObjectTooltipTextUpdated(SetEventPayload(newText));
            ResetTooltip();
        }

        protected virtual void OnEnable()
        {
            ResetTooltip();
            headset = InputAdapter.Instance.PlayerController.Nodes.Head.Transform;
        }


        protected virtual void LateUpdate()
        {
            DrawLine();
            if (alwaysFaceHeadset)
            {
                transform.LookAt(headset);
            }

            if (headset && tooltipText && tooltipReverseText)
            {
                var dir = headset.position - tooltipText.transform.position;
                var forward = Vector3.Dot(headset.forward, dir) > 0;
                tooltipText.enabled = forward;
                tooltipReverseText.enabled = !forward;
            }
        }

        protected virtual ObjectTooltipEventArgs SetEventPayload(string newText = "")
        {
            ObjectTooltipEventArgs e;
            e.newText = newText;
            return e;
        }

        protected virtual void SetContainer()
        {
            Transform tmpContainer = transform.Find("TooltipCanvas/UIContainer");
            tmpContainer.GetComponent<Image>().color = containerColor;
        }

        protected virtual void SetText(string name)
        {
            Text tmpText = transform.Find("TooltipCanvas/" + name).GetComponent<Text>();
            tmpText.material = Resources.Load("UIText") as Material;
            tmpText.text = displayText.Replace("\\n", "\n");
            tmpText.color = fontColor;
        }

        protected virtual void SetLine()
        {
            line = transform.Find("Line").GetComponent<LineRenderer>();
            line.material = Resources.Load("TooltipLine") as Material;
            line.material.color = lineColor;
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;

            if (!drawLineFrom)
            {
                drawLineFrom = transform;
            }
        }

        protected virtual void DrawLine()
        {
            if (drawLineTo)
            {
                line.SetPosition(0, drawLineFrom.position);
                line.SetPosition(1, drawLineTo.position);
            }
        }
    }
