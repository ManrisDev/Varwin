using System;
using UnityEngine;

namespace Varwin.Core
{
    public static class Logger
    {
        public static void LogVerbose(string message)
        {
            Debug.Log(message);
        }
        
        public static void LogMessage(string message)
        {
            Debug.Log(message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);   
        }
        
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}