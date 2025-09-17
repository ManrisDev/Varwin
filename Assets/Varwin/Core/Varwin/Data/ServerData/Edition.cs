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
        EducationKazakh
    }
}