using UnityEngine;
using Varwin;
using Varwin.Public;
using Varwin.Types.Keyboard_3494a171b33e48ce8e1f40b94334a47c;

public class Button : MonoBehaviour, IUseStartInteractionAware
{
    public KeyValue KeyValue;

    private Keyboard _keyboard;
    private Animation _animation;

    private void Awake()
    {
        _keyboard = GetComponentInParent<Keyboard>();
        _animation = GetComponent<Animation>();
    }

    private void Update()
    {
        //if (Input.GetKeyUp(KeyCode.Escape))
        //{
        //    _animation.Play();
        //}
    }
    public void OnUseStart(UseInteractionContext context)
    {
        _keyboard.OnKeyboardPressed(KeyValue);
        _animation.Play();
    }
}
