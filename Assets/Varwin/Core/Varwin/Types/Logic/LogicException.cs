using System;
using System.Diagnostics;
using System.Reflection;
using Varwin.Log;
#if VARWINCLIENT
using Varwin.UI.VRErrorManager;
using Varwin.WWW;
#endif

namespace Varwin.Types
{
    public class LogicException : Exception
    {
        public int SceneId { get; }
        public MethodBase Method { get; }
        public string MethodName { get; }
        public string DeclaringTypeFullName { get; }

        private readonly StackFrame _frame;
        public int Line => _frame?.GetFileLineNumber() ?? 0;
        public int Column => _frame?.GetFileColumnNumber() ?? 0;

        public new readonly string Message;

        public LogicException(int sceneId, string errorMsg, Exception exception) : base(errorMsg, exception)
        {
            SceneId = sceneId;

            _frame = GetStackTraceFrame(exception);

            Method = _frame.GetMethod();

            if (Method != null)
            {
                MethodName = Method.Name;

                if (Method.DeclaringType != null)
                {
                    DeclaringTypeFullName = Method.DeclaringType.FullName;
                }
            }

            if (!string.IsNullOrEmpty(DeclaringTypeFullName) && !DeclaringTypeFullName.Contains("Varwin.LogicOfScene"))
            {
                string objectType = DeclaringTypeFullName.Split('_')[0];
                Message = $"Object {objectType} has error in method {MethodName}. Exception = {exception.Message}";
            }
            else
            {
                Message = exception.Message;
            }
        }

        public string GetStackFrameString() => InnerException != null
            ? $"Scene Id = {SceneId}; Line: {Line}; Column: {Column}; Message:{InnerException.Message}"
            : null;

        public static StackFrame GetStackTraceFrame(Exception exception)
        {
            var st = new StackTrace(exception, true);

            return st.GetFrame(0);
        }
    }
}
