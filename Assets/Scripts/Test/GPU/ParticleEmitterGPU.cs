namespace Eden.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;

    public class ParticleEmitterGPU : IParticleEmitterGPU
    {
        #region Fields
        [SerializeField] private bool _SpawnParticles = true;

        [SerializeField] private float _particleMinFadeout = 1.0f;
        [SerializeField] private float _particleLifeMin = 3.0f;
        [SerializeField] private float _particleLifeMax = 9.0f;
        [SerializeField] private float _particlesBySec = 10;
        [SerializeField] private GameObject _particlePrefab = null;
        [SerializeField] private float _maxAngle = 30.0f;
        [SerializeField] private float _particleSpeedMin = 10.0f;
        [SerializeField] private float _particleSpeedMax = 10.0f;
        [SerializeField] private float _deceleration = 10.0f;
        [SerializeField] private bool _moveEmitter = false;

        // Particle
        [SerializeField] private float _colorChangeDurationMin = 0.3f;
        [SerializeField] private float _colorChangeDurationMax = 1.5f;
        private bool _changeColor = false;




        private float _particlesToSpawn = 0.0f;


        private uint ID = 0;
        #endregion Fields

        #region Methods

        private void Do_CS_ParticlesSpawn()
        {
            int nb_spawn = (int)_particlesToSpawn;
            _particlesToSpawn -= nb_spawn;

            if (_SpawnParticles && nb_spawn > 0)
            {
                //ComputeShaderParticlesSpawn.SetBuffer(kernel, "_Particules", particlesBuffer);
                InitRandomCS(ref CSParticlesSpawn);

                CSParticlesSpawn.SetInt("NB_Spawn", nb_spawn);
                CSParticlesSpawn.SetInt("offset", (int)ID);
                CSParticlesSpawn.SetInt("MaxParticules", get_MaxParticulesNB);

                CSParticlesSpawn.SetFloat("_colorChangeDurationMin", _colorChangeDurationMin);
                CSParticlesSpawn.SetFloat("_colorChangeDurationMax", _colorChangeDurationMax);
                CSParticlesSpawn.SetBool("ChangeColor", _changeColor);


                CSParticlesSpawn.SetFloat("_particleMinFadeout", _particleMinFadeout);

                CSParticlesSpawn.SetFloat("_particleLifeMin", _particleLifeMin);
                CSParticlesSpawn.SetFloat("_particleLifeMax", _particleLifeMax);

                CSParticlesSpawn.SetFloat("_particleSpeedMin", _particleSpeedMin);
                CSParticlesSpawn.SetFloat("_particleSpeedMax", _particleSpeedMax);
                CSParticlesSpawn.SetFloat("_deceleration", _deceleration);

                Quaternion max = Quaternion.Euler(0.0f, 0.0f, _maxAngle);
                Quaternion min = Quaternion.Euler(0.0f, 0.0f, -_maxAngle);

                Vector3 maxVector = max * transform.up;
                Vector3 minVector = min * transform.up;

                CSParticlesSpawn.SetVector("minVector", minVector);
                CSParticlesSpawn.SetVector("maxVector", maxVector);
                CSParticlesSpawn.SetVector("origine", transform.position);


                Dispatch_CSParticlesSpawn(nb_spawn);
                ID = (uint)((ID + nb_spawn) % get_MaxParticulesNB);
            }
        }


        private void Do_CS_ParticlesUpdate()
        {
            //if (alive == 0) return;

            // same for the float values
            InitRandomCS(ref CSParticlesUpdate);
            CSParticlesUpdate.SetFloat("deltaTime", Time.deltaTime);

            ResetCountBuffer();

            // start the compute shader and wait
            Dispatch_CSParticlesUpdate();

            // request Async count of alive particles (avoid useless wait for getData from Buffer)
            AsyncCountAlive();
        }


        protected void Start()
        {
            CopyNewParam();

            InitializeSize(Mathf.CeilToInt(_particlesBySec * _particleLifeMax));

            InitializeMesh(ref _particlePrefab);
            InitializeBuffers();
        }


        protected void Update()
        {
            if (_moveEmitter)
                transform.localPosition += new Vector3(Mathf.Cos(Time.time / 2) * 3, Mathf.Sin(Time.time / 2) * 3, 0) * Time.deltaTime;


            _particlesToSpawn += _particlesBySec * Time.deltaTime;

            Do_CS_ParticlesSpawn();

            Do_CS_ParticlesUpdate();

            drawParticles();
        }


        #endregion Methods
    }
}

