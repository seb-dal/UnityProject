namespace Eden.Test
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class ParticleBombGPU : IParticleEmitterGPU
    {

        [SerializeField] private int _particleCount = 100;
        [SerializeField] private GameObject _particlePrefab = null;
        [SerializeField] private float _deceleration = 10.0f;
        [SerializeField] private float _speedMin = 600.0f;

        [SerializeField] private float _speedMax = 900.0f;
        [SerializeField] private Color _startColor = Color.yellow;
        [SerializeField] private Color _endColor = Color.red;


        public void Set_particlePrefab(GameObject go) { _particlePrefab = go; }


        public void Initialize()
        {

            //CSParticlesSpawn = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/Test/CSPartclesBombSpawn.compute");
            //CSParticlesUpdate = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/Test/CSParticleUpdate.compute");

            CopyNewParam();

            InitializeSize(_particleCount);
            InitializeMesh(ref _particlePrefab);
            InitializeBuffers();
        }


        public void Explode()
        {
            InitRandomCS(ref CSParticlesSpawn);

            CSParticlesSpawn.SetVector("_startColor", _startColor);
            CSParticlesSpawn.SetVector("_endColor", _endColor);
            CSParticlesSpawn.SetVector("origine", transform.position);

            CSParticlesSpawn.SetFloat("_speedMin", _speedMin);
            CSParticlesSpawn.SetFloat("_speedMax", _speedMax);
            CSParticlesSpawn.SetFloat("_deceleration", _deceleration);

            Dispatch_CSParticlesSpawn();
        }


        public void Update()
        {
            InitRandomCS(ref CSParticlesUpdate);

            CSParticlesUpdate.SetFloat("deltaTime", Time.deltaTime);

            ResetCountBuffer();
            // start the compute shader and wait
            Dispatch_CSParticlesUpdate();

            // request Async count of alive particles (avoid useless wait for getData from Buffer)
            AsyncCountAlive();

            drawParticles();
        }

    }
}