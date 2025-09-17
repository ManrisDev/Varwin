// ReSharper disable All

using System;
using UnityEngine;
using Varwin.Data;

namespace Varwin.Models.Data
{
    public class TransformDT : IJsonSerializable
    {
        public Vector3DT PositionDT = new Vector3DT();
        public QuaternionDT RotationDT = new QuaternionDT();
        public Vector3DT ScaleDT = new Vector3DT();

        private Vector3DT _eulerAnglesDT = new Vector3DT();
        
        public Vector3DT EulerAnglesDT
        {
            get
            {
                return _eulerAnglesDT;
            }
            set
            {
                RotationDT.SetQuaternion(Quaternion.Euler(value));
                _eulerAnglesDT = value;
            }
        }
        
        public static implicit operator TransformDT(Transform t) => t.ToTransformDT();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            TransformDT dto = obj as TransformDT;
            return PositionDT == dto.PositionDT && ScaleDT == dto.ScaleDT && RotationDT == dto.RotationDT;
        }

        public static bool operator ==(TransformDT lhs, TransformDT rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(TransformDT lhs, TransformDT rhs) => !(lhs == rhs);
    }

    public class Vector3DT : IJsonSerializable
    {
        [NonSerialized]
        private const float ZeroScaleValue = 0.000001f;

        public float x;
        public float y;
        public float z;
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            
            Vector3DT dt = obj as Vector3DT;
            return (Vector3) this == (Vector3) dt;
        }
        
        public static bool operator ==(Vector3DT lhs, Vector3DT rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Vector3DT lhs, Vector3DT rhs) => !(lhs == rhs);
        
        public Vector3DT()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector3DT(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Vector3DT(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public void SetZeroScaleValues()
        {
            if (x == 0)
            {
                x = ZeroScaleValue;
            }

            if (y == 0)
            {
                y = ZeroScaleValue;
            }

            if (z == 0)
            {
                z = ZeroScaleValue;
            }
        }

        public static implicit operator Vector3(Vector3DT vector3Dt) => new Vector3(vector3Dt.x, vector3Dt.y, vector3Dt.z);
        public static implicit operator Vector3DT(Vector3 vector3) => new Vector3DT(vector3);

        public override string ToString()
        {
            return $"{(Vector3) this}";
        }
    }

    public class QuaternionDT : IJsonSerializable
    {
        public float x;
        public float y;
        public float z;
        public float w;
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            QuaternionDT dt = obj as QuaternionDT;
            return Mathf.Abs( 1f - Dot(this, dt)) < Mathf.Epsilon;
        }

        public static float Dot(QuaternionDT first, QuaternionDT second)
        {
            return first.x * second.x + first.y * second.y + first.z * second.z + first.w * second.w;
        }
        
        public static bool operator ==(QuaternionDT lhs, QuaternionDT rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(QuaternionDT lhs, QuaternionDT rhs) => !(lhs == rhs);

        public QuaternionDT()
        {
            
        }

        public QuaternionDT(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
        
        public static implicit operator Quaternion(QuaternionDT q) => new Quaternion(q.x, q.y, q.z, q.w);
        public static implicit operator QuaternionDT(Quaternion q) => new QuaternionDT(q);

        public override string ToString()
        {
            return $"{(Quaternion) this}";
        }
    }
}
