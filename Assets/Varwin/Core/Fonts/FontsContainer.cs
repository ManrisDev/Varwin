using System.Linq;
using TMPro;
using UnityEngine;

namespace Varwin
{
    public static class FontsContainer
    {
        private const string UbuntuRegularPath = "Ubuntu/Ubuntu-Regular SDF";
        private const string PTSerifRegularPath = "PTSerif/PTSerif-Regular SDF";
        private const string RobotoMonoRegularPath = "RobotoMono/RobotoMono-Regular SDF";
        private const string BadScriptRegularPath = "BadScript/BadScript-Regular SDF";

        private static TMP_FontAsset Ubuntu;
        private static TMP_FontAsset PtSerif;
        private static TMP_FontAsset RobotoMono;
        private static TMP_FontAsset BadScript;

        static FontsContainer()
        {
            Initialize();
        }

        public static TMP_FontAsset GetFont(Fonts font)
        {
            return font switch
            {
                Fonts.Ubuntu        => Ubuntu,
                Fonts.PtSerif       => PtSerif,
                Fonts.RobotoMono    => RobotoMono,
                Fonts.BadScript     => BadScript,
                _                   => Ubuntu
            };
        }

        public enum Fonts
        {
            [Item(English:"Ubuntu",Russian:"Ubuntu",Chinese:"Ubuntu",Kazakh:"Ubuntu",Korean:"Ubuntu")]
            Ubuntu,
            [Item(English:"PT Serif",Russian:"PT Serif",Chinese:"Pt Serif",Kazakh:"PT Serif",Korean:"PT Serif")]
            PtSerif,
            [Item(English:"Roboto Mono",Russian:"Roboto Mono",Chinese:"Roboto Mono",Kazakh:"Roboto Mono",Korean:"Roboto Mono")]
            RobotoMono,
            [Item(English:"BadScript",Russian:"BadScript",Chinese:"BadScript",Kazakh:"BadScript",Korean:"BadScript")]
            BadScript
        }

        private static void Initialize()
        {
            Ubuntu = Resources.Load<TMP_FontAsset>(UbuntuRegularPath);
            PtSerif = Resources.Load<TMP_FontAsset>(PTSerifRegularPath);
            RobotoMono = Resources.Load<TMP_FontAsset>(RobotoMonoRegularPath);
            BadScript = Resources.Load<TMP_FontAsset>(BadScriptRegularPath);
        }
    }
}
