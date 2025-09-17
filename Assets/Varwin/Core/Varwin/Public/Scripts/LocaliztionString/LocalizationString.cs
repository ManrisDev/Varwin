using UnityEngine;

namespace Varwin.Public
{
    [System.Serializable]
    public class LocalizationString
    {
        public Language key;
        
        public string value;

        public LocalizationString() { }

        public LocalizationString(Language key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}