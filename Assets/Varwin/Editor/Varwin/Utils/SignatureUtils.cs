using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public static class SignatureUtils
    {
        
        private static readonly string[] IgnoredNamespaceStarts =
        {
            "UnityEngine.",
            "TMPro.",
        };
        
        private static readonly Type[] SignatureAwareAttributes =
        {
            typeof(GetterAttribute), typeof(SetterAttribute),
            typeof(VariableAttribute), typeof(VariableGroupAttribute),
            typeof(ActionAttribute), typeof(ActionGroupAttribute),
            typeof(CheckerAttribute), typeof(CheckerGroupAttribute),
            typeof(FunctionAttribute), typeof(FunctionGroupAttribute),
            typeof(EventAttribute), typeof(EventGroupAttribute),
            typeof(ValueListAttribute), typeof(UseValueListAttribute)
        };

        public static void SetupObjectSignatures(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            varwinObjectDescriptor.Signatures = MakeSignatures(varwinObjectDescriptor);
        }

        public static SignatureCollection MakeSignatures(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            var processedTypes = new HashSet<Type>();
            
            var signatureContainer = new SignatureCollection();

            foreach (ComponentReference componentReference in varwinObjectDescriptor.Components)
            {
                if (componentReference == null || !componentReference.Component)
                {
                    continue;
                }
                
                if (!processedTypes.Add(componentReference.Type))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(componentReference.Type.FullName) && IgnoredNamespaceStarts.Any(x => componentReference.Type.FullName.StartsWith(x)))
                {
                    continue;
                }
                
                MemberInfo[] members = GetComponentMembers(componentReference).ToArray();

                signatureContainer.PropertySignatures.AddRange(CollectPropertySignatures(componentReference, members));
                signatureContainer.MethodSignatures.AddRange(CollectMethodSignatures(componentReference, members));
                signatureContainer.EventSignatures.AddRange(CollectEventSignatures(componentReference, members));
                signatureContainer.UseValueListSignatures.AddRange(CollectUseValueListSignatures(componentReference, members));
                signatureContainer.GroupSignatures.AddRange(CollectGroups(componentReference, members));
            }

            return signatureContainer;
        }

        private static MethodSignature MakeMethodSignature(ComponentReference componentReference, MethodInfo methodInfo)
        {
            return new MethodSignature
            {
                ReturnType = GetFriendlyTypeName(methodInfo.ReturnType),
                Name = MakeSignatureName(componentReference, methodInfo.Name),
                Parameters = methodInfo.GetParameters().Select(x => GetFriendlyTypeName(x.ParameterType)).ToList()
            };
        }

        private static MethodSignature MakeEventSignature(ComponentReference componentReference, EventInfo eventInfo)
        {
            Type delegateType = eventInfo.EventHandlerType;
            MethodInfo invokeInfo = delegateType.GetMethod("Invoke");
            if (invokeInfo == null)
            {
                return null;
            }

            return new MethodSignature
            {
                ReturnType = GetFriendlyTypeName(invokeInfo.ReturnType),
                Name = MakeSignatureName(componentReference, eventInfo.Name),
                Parameters = invokeInfo.GetParameters().Select(x => x.ParameterType.Name).ToList()
            };
        }

        private static IEnumerable<PropertySignature> CollectPropertySignatures(ComponentReference componentReference, IEnumerable<MemberInfo> members)
        {
            var variableSignatures = new List<PropertySignature>();
            foreach (MemberInfo memberInfo in members)
            {
                var fieldInfo = memberInfo as FieldInfo;
                var propertyInfo = memberInfo as PropertyInfo;
                if (fieldInfo == null && propertyInfo == null)
                {
                    continue;
                }

                variableSignatures.Add(new PropertySignature
                {
                    Name = MakeSignatureName(componentReference, memberInfo.Name),
                    Type = fieldInfo != null
                        ? GetFriendlyTypeName(fieldInfo.FieldType)
                        : GetFriendlyTypeName(propertyInfo.PropertyType)
                });
            }

            return variableSignatures;
        }

        private static IEnumerable<MethodSignature> CollectMethodSignatures(ComponentReference componentReference, IEnumerable<MemberInfo> members)
        {
            var methodSignatures = new List<MethodSignature>();

            foreach (MemberInfo memberInfo in members)
            {
                if (!(memberInfo is MethodInfo methodInfo))
                {
                    continue;
                }

                methodSignatures.Add(MakeMethodSignature(componentReference, methodInfo));
            }

            return methodSignatures;
        }

        private static IEnumerable<MethodSignature> CollectEventSignatures(ComponentReference componentReference, IEnumerable<MemberInfo> members)
        {
            var eventSignatures = new List<MethodSignature>();

            foreach (MemberInfo memberInfo in members)
            {
                if (!(memberInfo is EventInfo eventInfo))
                {
                    continue;
                }

                MethodSignature eventSignature = MakeEventSignature(componentReference, eventInfo);
                if (eventSignature != null)
                {
                    eventSignatures.Add(eventSignature);
                }
            }

            return eventSignatures;
        }

        private static IEnumerable<UseValueListSignature> CollectUseValueListSignatures(ComponentReference componentReference, IEnumerable<MemberInfo> members)
        {
            var useValueListSignatures = new List<UseValueListSignature>();
            foreach (MemberInfo memberInfo in members)
            {
                if (!(memberInfo is MethodInfo methodInfo))
                {
                    continue;
                }

                ParameterInfo[] parameters = methodInfo.GetParameters();
                foreach (ParameterInfo parameter in parameters)
                {
                    var useValueListAttribute = parameter.GetCustomAttribute<UseValueListAttribute>();
                    if (useValueListAttribute == null)
                    {
                        continue;
                    }

                    useValueListSignatures.Add(new UseValueListSignature
                    {
                        Name = $"{methodInfo.Name}.{MakeSignatureName(componentReference, parameter.Name)}",
                        Members = useValueListAttribute.ListNames
                    });
                }
            }

            return useValueListSignatures;
        }

        private static IEnumerable<GroupSignature> CollectGroups(ComponentReference componentReference, IEnumerable<MemberInfo> members)
        {
            var storedGroups = new Dictionary<GroupAttribute, List<MemberInfo>>();

            foreach (MemberInfo memberInfo in members)
            {
                var groupAttribute = memberInfo.GetCustomAttribute<GroupAttribute>();
                if (groupAttribute == null)
                {
                    continue;
                }

                if (!storedGroups.ContainsKey(groupAttribute) || storedGroups[groupAttribute] == null)
                {
                    storedGroups[groupAttribute] = new List<MemberInfo>();
                }

                storedGroups[groupAttribute].Add(memberInfo);
            }

            var groupSignatures = new List<GroupSignature>();

            foreach (var storedGroup in storedGroups)
            {
                var groupSignature = new GroupSignature
                {
                    Name = MakeSignatureName(componentReference, storedGroup.Key.GroupName),
                    Members = new List<string>()
                };

                foreach (MemberInfo memberInfo in storedGroup.Value)
                {
                    groupSignature.Members.Add(MakeSignatureName(componentReference, memberInfo.Name));
                }

                groupSignatures.Add(groupSignature);
            }

            return groupSignatures;
        }

        private static IEnumerable<MemberInfo> GetComponentMembers(ComponentReference reference)
        {
            return reference.Component
                .GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member => SignatureAwareAttributes.Any(attribute => member.GetCustomAttribute(attribute, true) != null));
        }

        private static string MakeSignatureName(ComponentReference componentReference, string infoName)
        {
            return $"{componentReference.Name}.{infoName}";
        }

        private static string MakePropertyDiffWarning(KeyValuePair<Signature, Signature> signatureDiff)
        {
            var originalProperty = signatureDiff.Key as PropertySignature;

            string status = !(signatureDiff.Value is PropertySignature newProperty)
                ? "is no longer exist"
                : $"type was changed from \"{originalProperty.Type}\" to \"{newProperty.Type}\"";

            return $"Property \"{originalProperty.Name}\" {status}";
        }

        private static string MakeMethodDiffWarning(KeyValuePair<Signature, Signature> signatureDiff)
        {
            var originalMethod = signatureDiff.Key as MethodSignature;

            string status = !(signatureDiff.Value is MethodSignature newMethod)
                ? "is no longer exist"
                : $"signature was changed from \"{originalMethod}\" to \"{newMethod}\"";

            return $"Method \"{originalMethod.Name}\" {status}";
        }

        private static string MakeGroupDiffWarning(KeyValuePair<Signature, Signature> signatureDiff)
        {
            var originalGroup = signatureDiff.Key as GroupSignature;
            string status;
            if (signatureDiff.Value is GroupSignature newGroup)
            {
                List<string> memberDiff = originalGroup.Members.Except(newGroup.Members).ToList();
                status = $"has missing members: {memberDiff.Aggregate((current, param) => current + ", " + param)}";
            }
            else
            {
                status = "is no longer exist";
            }

            return $"Group \"{originalGroup.Name}\" {status}";
        }

        private static string MakeUseValueListDiffWarning(KeyValuePair<Signature, Signature> signatureDiff)
        {
            var originalUseValueList = signatureDiff.Key as UseValueListSignature;
            string status;
            if (signatureDiff.Value is UseValueListSignature newUseValueList)
            {
                List<string> memberDiff = originalUseValueList.Members.Except(newUseValueList.Members).ToList();
                status = $"has missing members: {memberDiff.Aggregate((current, param) => current + ", " + param)}";
            }
            else
            {
                status = "is no longer exist";
            }

            return $"UseValueList \"{originalUseValueList.Name}\" {status}";
        }

        public static string MakeSignatureWarning(KeyValuePair<Signature, Signature> signatureDiff)
        {
            if (signatureDiff.Key is PropertySignature)
            {
                return MakePropertyDiffWarning(signatureDiff);
            }

            if (signatureDiff.Key is MethodSignature)
            {
                return MakeMethodDiffWarning(signatureDiff);
            }

            if (signatureDiff.Key is GroupSignature)
            {
                return MakeGroupDiffWarning(signatureDiff);
            }

            if (signatureDiff.Key is UseValueListSignature)
            {
                return MakeUseValueListDiffWarning(signatureDiff);
            }

            return null;
        }

        private static string GetFriendlyTypeName(Type type)
        {
            using (var provider = new CSharpCodeProvider())
            {
                var typeRef = new CodeTypeReference(type);
                return provider.GetTypeOutput(typeRef);
            }
        }
    }
}