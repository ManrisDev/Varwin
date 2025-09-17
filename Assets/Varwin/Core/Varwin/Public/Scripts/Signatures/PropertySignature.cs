using System;

namespace Varwin.Public
{
    [Serializable]
    public class PropertySignature : Signature
    {
        public string Type;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as PropertySignature;

            return Name == other.Name && Type == other.Type;
        }

        protected bool Equals(PropertySignature other)
        {
            return Name == other.Name && Type == other.Type;
        }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}