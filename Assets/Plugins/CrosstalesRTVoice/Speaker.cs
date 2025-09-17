using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crosstales.RTVoice
{

    public class Speaker
    {
        public static bool isCustomMode;

        public static List<Model.Voice> Voices;
        
        public static Model.Voice VoiceForGender(
            Model.Enum.Gender gender,
            string culture = "",
            int index = 0,
            string fallbackCulture = "",
            bool isFuzzy = true)
        {
            return null;
        }

        public static Model.Voice VoiceForCulture(
            string culture,
            int index = 0,
            string fallbackCulture = "",
            bool isFuzzy = true)
        {
            return null;
        }

        public static Model.Voice VoiceForName(string name, bool isExact = false)
        {
            return null;
        }

        public static string Speak(
            string text,
            AudioSource source = null,
            Model.Voice voice = null,
            bool speakImmediately = true,
            float rate = 1f,
            float pitch = 1f,
            float volume = 1f,
            string outputFile = "",
            bool forceSSML = true)
        {
            return null;
        }
        
        public delegate void SpeakComplete(Model.Wrapper wrapper);
        public static event SpeakComplete OnSpeakComplete;

        public static void Silence()
        {
        }
    }
}

namespace Crosstales.RTVoice.Model
{
    public class Voice
    {
       
    }
    
    public class Wrapper
    {
        public string Uid;
    }
}

namespace Crosstales.RTVoice.Model.Enum
{
    public enum Gender
    {
        MALE,
        FEMALE,
        UNKNOWN
    }
}