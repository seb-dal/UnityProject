namespace Eden.Test
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Eden.Tools;

    public class Main : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Text _info = null;
        [SerializeField] private FpsCounter _fpsCounter = null;

        private List<IParticleEmitter> _emitters = new List<IParticleEmitter>();
        private static Main _instance = null;
        #endregion Fields

        #region Methods
        public static Main Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Main>();

                return _instance;
            }
        }

        public static bool HasInstance { get { return _instance != null; } }

        public void RegisterEmitter(IParticleEmitter emitter)
        {
            _emitters.Add(emitter);
        }

        public void UnregisterEmitter(IParticleEmitter emitter)
        {
            _emitters.Remove(emitter);
        }

        private void Start()
        {
            StartCoroutine(UpdateInfo(1.0f));
        }

        private IEnumerator UpdateInfo(float refreshInterval)
        {

            while (true)
            {

                int liveParticles = 0;

                foreach (IParticleEmitter emitter in _emitters)
                    liveParticles += emitter.ParticleCount;


                _info.text = string.Empty;
                _info.text += "FPS : " + _fpsCounter.Fps;
                _info.text += "\nParticles count : " + liveParticles;

                // the same as before but we don't do unnecessary computation
                yield return new WaitForSeconds(refreshInterval);
            }
        }

        public void delete50()
        {
            foreach (IParticleEmitter emitter in _emitters) { emitter.Delete50(); }
        }
        #endregion Methods
    }
}
