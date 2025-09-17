namespace Varwin.Editor
{
    public class GenericValueDescriptor
    {
        public string FieldName;
        public I18n Locale;
        public string FullTypeName;

        public GenericValueDescriptor(){}
        
        public GenericValueDescriptor(string fieldName, I18n locale)
        {
            FieldName = fieldName;
            Locale = locale;
        }
    }
}