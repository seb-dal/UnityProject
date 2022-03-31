namespace Eden.Test
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Assertions;

    public abstract class IParticleEmitter : MonoBehaviour
    {
        public abstract int ParticleCount { get; }

        protected virtual void OnEnable() { Assert.IsTrue(TryRegisterEmitter()); }

        protected virtual void OnDisable() { TryUnregisterEmitter(); }

        private bool TryRegisterEmitter()
        {
            if (Main.Instance != null)
            {
                Main.Instance.RegisterEmitter(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryUnregisterEmitter()
        {
            if (Main.Instance != null)
            {
                Main.Instance.UnregisterEmitter(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract void Delete50();
    }
}