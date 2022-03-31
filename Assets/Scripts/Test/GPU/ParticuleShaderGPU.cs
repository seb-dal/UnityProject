namespace Eden.Test.BufferMath
{
    public class Vector2 { public static int Size() { return sizeof(float) * 2; } };
    public class Vector3 { public static int Size() { return sizeof(float) * 3; } };
    public class Vector4 { public static int Size() { return sizeof(float) * 4; } };
    public class Matrix4x4 { public static int Size() { return sizeof(float) * 4 * 4; } };
};

namespace Eden.Test
{


    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    /**
     * Data structure of a Particle on GPU. The structure must be the same on Shader and Compute Shader to work.
     */
    public struct ParticuleShaderGPU
    {

        public Vector3 _speed;

        // Position Scale Rotation matrix
        public Matrix4x4 mat;

        public float _lifespanMax;
        public float _lifespan;

        public float _fadeDurationInit;
        public float _fadeDuration;

        public float _colorChangeTime;
        public float _colorChangeDuration;

        public Vector4 _previousColor;
        public Vector4 _nextColor;
        public Vector4 _spriteColor;


        public float _deceleration;
        public int _changeColor;


        public static int Size()
        {
            return
                3 * BufferMath.Vector4.Size() +
                BufferMath.Vector3.Size() +
                BufferMath.Matrix4x4.Size() +
                7 * sizeof(float) +
                sizeof(int);
        }
    };

}