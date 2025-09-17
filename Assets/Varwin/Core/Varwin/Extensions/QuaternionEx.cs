using UnityEngine;

namespace Core.Varwin.Extensions
{
    public static class QuaternionEx
    {
        /// <summary>
        /// Get angle in range [-180, 180] between two quaternions around the Y axis
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static float GetSignedYAngleBetween(this Quaternion q1, Quaternion q2)
        {
            var deltaQuaternion = q1.Delta(q2);

            float signedAngle = Mathf.Rad2Deg * Mathf.Atan2(2 * (deltaQuaternion.y * deltaQuaternion.w + deltaQuaternion.x * deltaQuaternion.z),
                1 - 2 * (deltaQuaternion.y * deltaQuaternion.y + deltaQuaternion.z * deltaQuaternion.z));

            // Ensure the angle is in the range [-180, 180]
            signedAngle = Mathf.Repeat(signedAngle + 180f, 360f) - 180f;

            return signedAngle;
        }

        /// <summary>
        /// Calculates the delta from quaternion 1 to quaternion 2.
        /// </summary>
        /// <param name="q1">The first quaternion.</param>
        /// <param name="q2">The second quaternion.</param>
        /// <returns>The delta quaternion.</returns>
        public static Quaternion Delta(this Quaternion q1, Quaternion q2) => Quaternion.Inverse(q1) * q2;
    }
}