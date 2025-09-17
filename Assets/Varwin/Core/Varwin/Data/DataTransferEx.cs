using UnityEngine;

namespace Varwin.Models.Data
{
    public static class TransformEx
    {
        public static TransformDT Zero(this TransformDT self)
        {
            self = new TransformDT();
            self.PositionDT.Zero();
            self.RotationDT.Zero();
            self.ScaleDT.Zero();
            return self;
        }

        public static Vector3DT Zero(this Vector3DT self)
        {
            self = new Vector3DT();
            self.x = 0;
            self.y = 0;
            self.z = 0;
            return self;
        }

        public static QuaternionDT Zero(this QuaternionDT self)
        {
            self = new QuaternionDT();
            self.x = 0;
            self.y = 0;
            self.z = 0;
            self.w = 0;
            return self;
        }

        public static void SetVector3(this Vector3DT self, Vector3 vector3)
        {
            self.x = vector3.x;
            self.y = vector3.y;
            self.z = vector3.z;
        }

        public static void SetQuaternion(this QuaternionDT self, Quaternion quaternion)
        {
            self.x = quaternion.x;
            self.y = quaternion.y;
            self.z = quaternion.z;
            self.w = quaternion.w;
        }

        public static TransformDT ToTransformDT(this Transform self)
        {
            TransformDT transformDt = new TransformDT();
            transformDt.PositionDT.SetVector3(self.position);
            transformDt.EulerAnglesDT = self.eulerAngles;
            transformDt.ScaleDT.SetVector3(self.localScale);
            return transformDt;
        }

        public static TransformDT ToLocalTransformDT(this Transform self)
        {
            TransformDT transformDt = new TransformDT();
            transformDt.PositionDT.SetVector3(self.localPosition);
            transformDt.EulerAnglesDT = self.localEulerAngles;
            transformDt.ScaleDT.SetVector3(self.localScale);
            return transformDt;
        }

        public static void ToTransformUnity(this TransformDT self, Transform transform)
        {
            transform.position = new Vector3(self.PositionDT.x, self.PositionDT.y, self.PositionDT.z);
            transform.rotation = new Quaternion(self.RotationDT.x, self.RotationDT.y, self.RotationDT.z, self.RotationDT.w);
            transform.localScale = new Vector3(self.ScaleDT.x, self.ScaleDT.y, self.ScaleDT.z);
        }

        public static void ToLocalTransformUnity(this TransformDT self, Transform transform)
        {
            transform.localPosition = new Vector3(self.PositionDT.x, self.PositionDT.y, self.PositionDT.z);
            transform.localRotation = new Quaternion(self.RotationDT.x, self.RotationDT.y, self.RotationDT.z, self.RotationDT.w);
            transform.localScale = new Vector3(self.ScaleDT.x, self.ScaleDT.y, self.ScaleDT.z);
        }

        public static Vector3DT ToVector3Dt(this Vector3 self)
        {
            Vector3DT vector3Dt = new Vector3DT();
            vector3Dt.x = self.x;
            vector3Dt.y = self.y;
            vector3Dt.z = self.z;
            return vector3Dt;
        }

        public static Vector3 ToUnityVector(this Vector3DT self)
        {
            return new Vector3(self.x, self.y, self.z);
        }

        public static Quaternion ToUnityQuaternion(this QuaternionDT self)
        {
            return new Quaternion
            {
                x = self.x,
                y = self.y,
                z = self.z,
                w = self.w
            };
        }

        public static void CopyToTransform(this Transform self, Transform target)
        {
            target.position = self.position;
            target.rotation = self.rotation;
            target.localScale = self.localScale;
        }
    }
}