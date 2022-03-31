using Unity.Jobs;

namespace Eden.Test
{
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;

    public class ParticleEmitter : IParticleEmitter
    {
        #region Fields
        [SerializeField] private bool _SpawnParticles = true;

        [SerializeField] private float _particleMinFadeout = 1.0f;
        [SerializeField] private float _particleLifeMin = 3.0f;
        [SerializeField] private float _particleLifeMax = 9.0f;
        [SerializeField] private float _particlesBySec = 10;
        [SerializeField] private CSParticle _particlePrefab = null;
        [SerializeField] private GameObject _particlesContainer = null;
        [SerializeField] private float _maxAngle = 30.0f;
        [SerializeField] private float _particleSpeedMin = 10.0f;
        [SerializeField] private float _particleSpeedMax = 10.0f;
        [SerializeField] private float _deceleration = 10.0f;
        [SerializeField] private bool _moveEmitter = false;


        public ComputeShader ComputeShaderParticles;
        private int alive = 0;
        private ComputeBuffer particlesBuffer;
        private CSParticle[] _liveParticles;
        private ParticuleShader[] particleData;

        private float _particlesToSpawn = 0.0f;

        private uint MaxParticulesNB;
        private uint ID = 0;
        #endregion Fields

        #region Properties
        public override int ParticleCount { get { return alive; } }
        public ParticuleShader[] particles { get { return particleData; } }
        #endregion Properties

        #region Methods

        protected void Start()
        {
            /*
             we know the maximal life of a particule as well that the number of particles per seconds.
             So it's better to declare all the particles instead of creating and cestroying object.
             */
            MaxParticulesNB = (uint)Mathf.Ceil(_particleLifeMax * _particlesBySec);
            _liveParticles = new CSParticle[MaxParticulesNB];
            particleData = new ParticuleShader[MaxParticulesNB];

            int colorSize = sizeof(float) * 4;
            int vectorSize = sizeof(float) * 3;
            int sizeData = 2 * vectorSize + 3 * colorSize + 7 * sizeof(float) + sizeof(int);

            // cration of the buffer that will containe all the data of the particles to send to the ComputeShader
            // /!\ size of an element must be a multiple of 4 and between 0 and 2048 (no bool alone)
            particlesBuffer = new ComputeBuffer(particleData.Length, sizeData);

            // Place container position to (0; 0; 0) to have the particles 
            //if (_particlesContainer != null)
            // _particlesContainer.transform.position = Vector3.zero;

            for (int i = 0; i < MaxParticulesNB; i++)
            {
                // we Instantiate only once and we reuse them
                _liveParticles[i] = Instantiate(_particlePrefab);

                // Create the object as a child of a container to prevent a flood of gameObject in the list of Object (reduce lag when object hidden)
                if (_particlesContainer != null)
                    _liveParticles[i].gameObject.transform.SetParent(_particlesContainer.transform);

                // Hide new uninitialised particule
                _liveParticles[i].Hide();
            }
        }

        protected void Update()
        {
            if (_moveEmitter)
            {
                transform.localPosition += new Vector3(Mathf.Cos(Time.time / 2) * 3, Mathf.Sin(Time.time / 2) * 3, 0) * Time.deltaTime;
            }

            _particlesToSpawn += _particlesBySec * Time.deltaTime;

            while (_particlesToSpawn >= 1.0f)
            {
                if (_SpawnParticles) SpawnParticle();
                _particlesToSpawn -= 1.0f;
            }

            doComputeShader();

            applyChange();
        }

        private void doComputeShader()
        {
            if (alive == 0) return;
            // set the data to the buffer
            particlesBuffer.SetData(particleData);
            // we tell the compute shader to which variable to bind the buffer
            ComputeShaderParticles.SetBuffer(0, "_Particules", particlesBuffer);

            // same for the float values
            ComputeShaderParticles.SetFloat("deltaTime", Time.deltaTime);
            ComputeShaderParticles.SetFloat("time", Time.time);

            // start the compute shader and wait
            ComputeShaderParticles.Dispatch(0, particleData.Length / 32, 1, 1);

            // get back the data of the particles
            particlesBuffer.GetData(particleData);

        }

        private uint idPrev = 0;
        private void applyChange()
        {
            if (idPrev == ID && alive == 0) return;
            idPrev = ID;

            alive = 0;
            // a little bit of multithreading here to speed up the program 
            // But "transform" and other variables of the object are only mutable in the MainThread
            for (int i = 0; i < MaxParticulesNB; i++)
            {

                if (_liveParticles[i].IsDead())
                {
                    _liveParticles[i].Hide();
                    continue;
                }

                _liveParticles[i].Lifespan = particleData[i]._lifespan;

                // Setting color and position take long time
                // C++ would be better for this as it could be possible to reuse the buffer of color and position
                // directly in the draw shader and so avoid useless transfere of variables
                _liveParticles[i].transform.position = particleData[i]._position;

                _liveParticles[i].Sprite.color = particleData[i]._spriteColor;

                // we apply the data of the computeShader to the particles


                alive++;
            }
        }

        private void SpawnParticle()
        {
            uint id = ID = (ID + 1) % MaxParticulesNB;

            //Particle newParticle = Instantiate(_particlePrefab);

            float life = Random.Range(_particleLifeMin, _particleLifeMax);
            float fade = Random.Range(_particleMinFadeout, _particleLifeMin);
            Color color = Random.ColorHSV();

            Quaternion max = Quaternion.Euler(0.0f, 0.0f, _maxAngle);
            Quaternion min = Quaternion.Euler(0.0f, 0.0f, -_maxAngle);

            Vector3 maxVector = max * transform.up;
            Vector3 minVector = min * transform.up;

            Vector3 result = Vector3.Lerp(minVector, maxVector, Random.value);

            float particleSpeed = Random.Range(_particleSpeedMin, _particleSpeedMax);
            Vector3 speed = result * particleSpeed;

            _liveParticles[id].Init(life, fade, color, speed, _deceleration, true);

            // we init the data 
            particleData[id]._position = transform.position;
            particleData[id]._colorChangeTime = 0.0f;
            particleData[id]._lifespanMax = particleData[id]._lifespan = life;
            particleData[id]._fadeDurationInit = particleData[id]._fadeDuration = fade;

            particleData[id]._spriteColor = particleData[id]._previousColor = color;
            particleData[id]._colorChangeDuration = _liveParticles[id].ColorChangeDuration;
            particleData[id]._speed = speed;
            particleData[id]._deceleration = _deceleration;
            particleData[id]._changeColor = _liveParticles[id].ChangeColor ? 1 : 0;

            //_liveParticles[id].transform.SetParent(transform, false);
            _liveParticles[id].Show();
            //GameParticule.addParticule(newParticle);
            //_liveParticles.Add(newParticle);
        }

        public override void Delete50()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i]._lifespan > 0 && Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    particles[i]._lifespan = 0.0f;
                }
            }
        }
        #endregion Methods
    }
}

