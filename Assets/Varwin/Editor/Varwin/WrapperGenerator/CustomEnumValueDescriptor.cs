namespace Varwin.Editor
{
    public class CustomEnumValueDescriptor
    {
        public string FieldName;
        public I18n Locale;
        public I18n Group;
        public int Value;
        public string FullTypeName;
        public CustomEnumValueDescriptor(){}
        
        public CustomEnumValueDescriptor(string fieldName, I18n locale, int value, string fullTypeName, I18n group = null)
        {
            FieldName = fieldName;
            Locale = locale;
            Value = value;
            FullTypeName = fullTypeName;
            Group = group;
        }
    }
}