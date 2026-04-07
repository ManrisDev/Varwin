using System.Runtime.Serialization;

namespace Varwin
{
    public enum Edition
    {
        None,
        Starter,
        Professional,
        Business,
        Education,
        Robotics,
        Server,
        NettleDesk,
        Full,
        
        [EnumMember(Value = "education-korea")]
        EducationKorea,
        
        [EnumMember(Value = "education-kazakh")]
        EducationKazakh,
        
        [EnumMember(Value = "education-python")]
        EducationPython,
        
        [EnumMember(Value = "education-python-korea")]
        EducationPythonKorea,
        
        [EnumMember(Value = "education-python-kazakh")]
        EducationPythonKazakh,
        
        [EnumMember(Value = "nettledesk-python")]
        NettleDeskPython
    }
}