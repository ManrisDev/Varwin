using System;
using UnityEngine;

namespace Varwin.NettleDeskPlayer
{
    public class NettleDeskScreenPad : ScreenPadBase
    {
        public NettleDeskInteractionController NettleDeskInteractionController;
            
        private void Update()
        {
            float inputX = NettleDeskInteractionController.CursorPos.x / NettleDeskInteractionController.MonoRenderingController.ScreenWidth * 2 - 1;
            float inputY = NettleDeskInteractionController.CursorPos.y / NettleDeskInteractionController.MonoRenderingController.ScreenHeightMono * 2 - 1;
            Input = new Vector2(inputX, inputY);
        }
    }
}