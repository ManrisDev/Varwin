using System;
using System.Collections.Generic;

namespace Varwin.Public
{
    [Serializable]
    public class SignatureCollection
    {
        public List<PropertySignature> PropertySignatures = new List<PropertySignature>();
        public List<MethodSignature> MethodSignatures = new List<MethodSignature>();
        public List<MethodSignature> EventSignatures = new List<MethodSignature>();
        public List<GroupSignature> GroupSignatures = new List<GroupSignature>();
        public List<UseValueListSignature> UseValueListSignatures = new List<UseValueListSignature>();

        private static List<KeyValuePair<Signature, Signature>> GetAllSignatureDiffs<T>(IEnumerable<T> selfSignatures, IEnumerable<T> otherSignatures, Func<Signature, Signature, bool> comparator)
            where T : Signature
        {
            var signatureDiffs = new List<KeyValuePair<Signature, Signature>>();
            
            foreach (T otherSignature in otherSignatures)
            {
                var exist = false;
                foreach (T selfSignature in selfSignatures)
                {
                    if (selfSignature.Name != otherSignature.Name)
                    {
                        continue;
                    }

                    exist = true;
                    
                    if (comparator(selfSignature, otherSignature))
                    {
                        continue;
                    }
                    
                    signatureDiffs.Add(new KeyValuePair<Signature, Signature>(otherSignature, selfSignature));
                }

                if (!exist)
                {
                    signatureDiffs.Add(new KeyValuePair<Signature, Signature>(otherSignature, null));
                }
            }

            return signatureDiffs;
        }

        private static bool ComparePropertySignatures(Signature self, Signature other)
        {
            return self is PropertySignature selfMethod
                   && other is PropertySignature otherMethod
                   && selfMethod.Type == otherMethod.Type;
        }

        private static bool CompareMethodSignatures(Signature self, Signature other)
        {
            return self is MethodSignature selfMethod
                   && other is MethodSignature otherMethod
                   && selfMethod.ParametersAreEquals(otherMethod);
        }

        private static bool CompareGroupSignatures(Signature self, Signature other)
        {
            return self is GroupSignature selfGroup
                   && other is GroupSignature otherGroup
                   && selfGroup.ContainsAllMembersOf(otherGroup);
        }

        private static bool CompareUseValueListSignatures(Signature self, Signature other)
        {
            return self is UseValueListSignature selfUseValueList
                   && other is UseValueListSignature otherUseValueList
                   && selfUseValueList.ContainsAllMembersOf(otherUseValueList);
        }

        public List<KeyValuePair<Signature, Signature>> FindAllChangedSignatures(SignatureCollection other)
        {
            var signatureDiffs = new List<KeyValuePair<Signature, Signature>>();
            signatureDiffs.AddRange(GetAllSignatureDiffs(PropertySignatures, other.PropertySignatures, ComparePropertySignatures));
            signatureDiffs.AddRange(GetAllSignatureDiffs(MethodSignatures, other.MethodSignatures, CompareMethodSignatures));
            signatureDiffs.AddRange(GetAllSignatureDiffs(GroupSignatures, other.GroupSignatures, CompareGroupSignatures));
            signatureDiffs.AddRange(GetAllSignatureDiffs(UseValueListSignatures, other.UseValueListSignatures, CompareUseValueListSignatures));

            return signatureDiffs;
        }
    }
}