using UnityEngine;
using Varwin;
using Varwin.Public;

namespace Varwin.Types.Keyboard_3494a171b33e48ce8e1f40b94334a47c
{
    [VarwinComponent(English: "Keyboard")]
    public class Keyboard : VarwinObject
    {
        public delegate void PressKey(string keValueString);

        [LogicEvent(English: "Keyboard button is pressed")]
        public event PressKey PressKeyEvent;

        public void OnKeyboardPressed(KeyValue keyValue)
        {
            PressKeyEvent?.Invoke(keyValue.ToString());
        }
    }
}
