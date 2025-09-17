using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Varwin.Public;
using Varwin.Core.Behaviours;

namespace Varwin
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class Wrapper : DynamicObject
    {
        public event Action<string, object[]> ObservedMethodCalled;

        protected void OnObservedMethodCalled(string methodName, object[] args)
        {
            ObservedMethodCalled?.Invoke(methodName, args);
        }

        protected GameObject GameObject { get; set; }

        protected ObjectController ObjectController { get; set; }

        private bool _isEnabled = true;

        private bool _grabEnabled = true;

        public bool GrabEnabled
        {
            get => _grabEnabled;
            set => SwitchGrabEnabled(value);
        }

        private void SwitchGrabEnabled(bool value)
        {
            _grabEnabled = value;

            if (_grabEnabled)
            {
                EnableGrab();
            }
            else
            {
                DisableGrab();
            }
        }

        private bool _useEnabled = true;

        public bool UseEnabled
        {
            get => _useEnabled;
            set => SwitchUseEnabled(value);
        }

        private void SwitchUseEnabled(bool value)
        {
            _useEnabled = value;

            if (_useEnabled)
            {
                EnableUse();
            }
            else
            {
                DisableUse();
            }
        }

        private bool _touchEnabled = true;

        public bool TouchEnabled
        {
            get => _touchEnabled;
            set => SwitchTouchEnabled(value);
        }

        private void SwitchTouchEnabled(bool value)
        {
            _touchEnabled = value;

            if (_touchEnabled)
            {
                EnableTouch();
            }
            else
            {
                DisableTouch();
            }
        }
        
        private bool _pointerEnabled = true;

        public bool PointerEnabled
        {
            get => _pointerEnabled;
            set => SwitchPointerEnabled(value);
        }

        private void SwitchPointerEnabled(bool value)
        {
            _pointerEnabled = value;

            if (_pointerEnabled)
            {
                EnablePointer();
            }
            else
            {
                DisablePointer();
            }
        }


        protected Dictionary<Type, VarwinBehaviour> Behaviours { get; set; } = new();

        protected Wrapper(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        ~Wrapper()
        {
            Delete();
        }

        public void Delete()
        {
            SetEnabled(false);

            ObjectController?.Delete();
            ObjectController = null;
            if (GameObject)
            {
                UnityEngine.Object.Destroy(GameObject);
            }

            GameObject = null;
        }
        
        public void InitObjectController(ObjectController controller)
        {
            ObjectController = controller;
        }

        public void AddBehaviour(Type behaviourType, VarwinBehaviour behaviour)
        {
            Behaviours.TryAdd(behaviourType, behaviour);
        }

        public T GetBehaviour<T>() where T : VarwinBehaviour
        {
            return (T) GetBehaviour(typeof(T));
        }

        public VarwinBehaviour GetBehaviour(Type behaviourType)
        {
            return Behaviours.TryGetValue(behaviourType, out var behaviour) ? behaviour : null;
        }

        public IEnumerable<VarwinBehaviour> GetBehaviours()
        {
            return Behaviours.Values;
        }

        public bool IsActive()
        {
            return Activity;
        }

        public bool IsInactive()
        {
            return !Activity;
        }

        public void Activate()
        {
            Activity = true;
        }

        public void Deactivate()
        {
            Activity = false;
        }

        public bool Activity
        {
            get => ObjectController?.ActiveInHierarchy ?? false;
            set
            {
                if (!value)
                {
                    foreach (VarwinBehaviour behaviour in Behaviours.Values)
                    {
                        if (behaviour)
                        {
                            behaviour.StopAllCoroutines();
                        }
                    }
                }

                this.SetActivity(value);
            }
        }

        public bool Enabled
        {
            get => _isEnabled;
            set => SetEnabled(value);
        }

        private void SetEnabled(bool value)
        {
            if (value == _isEnabled)
            {
                return;
            }

            _isEnabled = value;

            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                if (_isEnabled)
                {
                    inputController?.EnableViewInput();
                }
                else
                {
                    inputController?.DisableViewInput();
                }
            }
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public bool IsEnabled() => _isEnabled;

        public bool IsDisabled() => !_isEnabled;

        public string GetName() => ObjectController.Name;

        public GameObject GetGameObject()
        {
            return GameObject ? GameObject : null;
        }

        public ObjectController GetObjectController() => ObjectController;

        public InputController GetInputController(GameObject go)
        {
            var objectId = go.GetComponent<ObjectId>();

            if (objectId == null)
            {
                return null;
            }
            
            var id = objectId.Id;
            return ObjectController.GetInputController(id);
        }

        #region INPUT LOGIC

        public void EnableUse()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.EnableViewUsing();
            }
        }

        public void DisableUse()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.DisableViewUsing();
            }
        }

        public void EnableTouch()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.EnableViewTouch();
            }
        }

        public void DisableTouch()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.DisableViewTouch();
            }
        }

        /// <summary>
        /// Enable grab for all grabbable in object
        /// </summary>
        public void EnableGrab()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.EnableViewGrab();
            }
        }

        /// <summary>
        /// Disable grab for all grabbable in objects
        /// </summary>
        public void DisableGrab()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.DisableViewGrab();
            }
        }
        
        public void EnablePointer()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.EnableViewPointer();
            }
        }
        
        public void DisablePointer()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.DisableViewPointer();
            }
        }
        
        public void EnableARTracking()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.EnableARTracking();
            }
        }
        
        public void DisableARTracking()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            var inputControllers = ObjectController.GetInputControllers();
            if (inputControllers == null)
            {
                return;
            }
            
            foreach (var inputController in inputControllers)
            {
                inputController?.DisableARTracking();
            }
        }

        public int GetInstanceId()
        {
           if (ObjectController is null)
           {
               Debug.LogError($"Can't execute {nameof(GetInstanceId)} of {GameObject}. Object Entity is not initialized");
               return default;
           }

           return ObjectController.Id;
        }

        public void EnableTouchForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.EnableViewTouch();
        }

        public void DisableTouchForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.DisableViewTouch();
        }
        
        public void EnableUseForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.EnableViewUsing();
        }

        public void DisableUseForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.DisableViewUsing();
        }
        
        public void EnableGrabForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.EnableViewGrab();
        }

        public void DisableGrabForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.DisableViewGrab();
        }

        public void EnablePointerForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.EnableViewPointer();
        }

        public void DisablePointerForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.DisableViewPointer();
        }
        
        public void EnableARTrackingForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.EnableARTracking();
        }

        public void DisableARTrackingForObject(GameObject go)
        {
            var control = GetInputController(go);

            control?.DisableARTracking();
        }

        public void VibrateWithObject(GameObject go, GameObject controllerObject, float strength, float duration, float interval)
        {
            var control = GetInputController(go);

            control?.Vibrate(controllerObject, strength, duration, interval);
        }

        #endregion

        #region Dynamic Methods

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = binder.Name;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Type thisType = GetType();
            FieldInfo fieldInfo = thisType.GetField(binder.Name);

            if (fieldInfo != null)
            {
                if (value.GetType() != fieldInfo.FieldType)
                {
                    Converter.CastValue(fieldInfo, value, this);
                }
            }
            else
            {
                PropertyInfo propertyInfo = thisType.GetProperty(binder.Name);

                if (propertyInfo == null)
                {
                    throw new Exception($"Missing property {binder.Name} in {GetType().FullName}");
                }

                if (value.GetType() != propertyInfo?.PropertyType)
                {
                    Converter.CastValue(propertyInfo, value, this);
                }
            }

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type thisType = GetType();
            MethodInfo methodInfo = thisType.GetMethod(binder.Name);

            if (methodInfo == null)
            {
                result = false;
                //Debug.Log("Method " + binder.Name + " not found!");
                return true;
            }

            var parametres = methodInfo.GetParameters();

            if (args.Length == parametres.Length)
            {
                bool error = false;
                for (int p = 0; p < args.Length; p++)
                {
                    var callArg = args[p];
                    var parametrInfo = parametres[p];

                    if (callArg.GetType() != parametrInfo.ParameterType)
                    {
                        if (!Converter.CastValue(parametrInfo.ParameterType, callArg, out callArg))
                        {
                            error = true;
                            break;
                        }

                        args[p] = callArg;
                    }
                }

                if (!error)
                {
                    methodInfo.Invoke(this, args);
                }
            }

            result = true;
            return true;
        }

        public bool HasProperty(string name)
        {
            Type thisType = GetType();
            var property = thisType.GetProperty(name);
            return property != null;
        }

        public bool HasField(string name)
        {
            Type thisType = GetType();
            var field = thisType.GetField(name);
            return field != null;
        }

        public bool HasMethod(string name)
        {
            Type thisType = GetType();
            var method = thisType.GetMethod(name);
            return method != null;
        }

        #endregion

        public void CallMethod(string methodName, params object[] parameters)
        {
            Type thisType = GetType();

            if (!HasMethod(methodName))
            {
                Debug.Log($"{thisType.Name} has no method {methodName}");

                return;
            }

            MethodInfo method = thisType.GetMethod(methodName);

            if (method == null)
            {
                return;
            }

            try
            {
                method.Invoke(this, parameters);
            }
            catch (Exception e)
            {
                Debug.LogError($"Can not invoke method \"{methodName}\" in {thisType.Name}: {e.Message}");
            }
        }

        public object GetValueFromValueList(ListValue listValue)
        {
            FieldInfo field = GetType().GetField(GetValidValueListName(listValue.ListName));

            if (field == null)
            {
                throw new Exception("dictionary not found for the valueList " + listValue.ListName);
            }

#if !NET_STANDARD_2_0
            try
            {
                object value = field.GetValue(this);
                dynamic dictionary = value;
                object result = dictionary[listValue.ValueName];

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Can not get value \"{listValue.ValueName}\" from valueList {listValue.ListName}: {e.Message}");
            }
#endif
            return null;
        }

        public static string GetValidValueListName(string valueListName)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9_]");
            string validValueListName = "ValueList_" + rgx.Replace(valueListName, "_");

            return validValueListName;
        }
    }

    public static class WrapperEx
    {
        [Obsolete("This will be removed in the future.", false)]
        public static void DisableInputUsing(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.DisableViewUsing();

        [Obsolete("This will be removed in the future.", false)]
        public static void EnableInputUsing(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.EnableViewUsing();

        [Obsolete("This will be removed in the future.", false)]
        public static void DisableInputGrab(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.DisableViewGrab();

        [Obsolete("This will be removed in the future.", false)]
        public static void EnableInputGrab(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.EnableViewGrab();

        [Obsolete("This will be removed in the future.", false)]
        public static void DisableTouch(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.DisableViewTouch();

        [Obsolete("This will be removed in the future.", false)]
        public static void EnableTouch(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.EnableViewTouch();
    }
}