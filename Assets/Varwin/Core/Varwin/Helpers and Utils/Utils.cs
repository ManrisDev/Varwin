using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin
{
    public static class Utils
    {
        public static string ConvertToString(object o)
        {
            return o.ToString();
        }

        #region Random Numbers Generator

        private static readonly System.Random RandomService = new System.Random();

        public static int RandomInt(int min, int max)
        {
            lock (RandomService)
            {
                return RandomService.Next(min, max + 1);
            }
        }

        public static double RandomDouble()
        {
            lock (RandomService)
            {
                return RandomService.NextDouble();
            }
        }

        public static float RandomFloat()
        {
            return Random.value;
        }

        /// <summary>
        /// Return true with specified probability (percentage)
        /// </summary>
        /// <param name="probabilityPercentage">Probability percentage (float number between 0 [inclusive] and 100 [inclusive])</param>
        /// <returns>True or false</returns>
        public static bool TrueWithProbability(float probabilityPercentage)
        {
            return Random.Range(Mathf.Epsilon, 100f - Mathf.Epsilon) <= probabilityPercentage;
        }

        /// <summary>
        /// Return true with specified probability (percentage)
        /// </summary>
        /// <param name="probabilityPercentage">Probability percentage (float number between 0 [inclusive] and 100 [inclusive])</param>
        /// <returns>True or false</returns>
        public static bool TrueWithProbability(double probabilityPercentage)
        {
            return TrueWithProbability((float) probabilityPercentage);
        }

        /// <summary>
        /// Return true with specified probability (percentage)
        /// </summary>
        /// <param name="probabilityPercentage">Probability percentage (float number between 0 [inclusive] and 100 [inclusive])</param>
        /// <returns>True or false</returns>
        public static bool TrueWithProbability(dynamic probabilityPercentage)
        {
#if !NET_STANDARD_2_0
            return TrueWithProbability(DynamicCast.ConvertToFloat(probabilityPercentage));
#else
            return Random.value > 0.5f;
#endif
        }

        #endregion
    }
}