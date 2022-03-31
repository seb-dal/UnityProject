namespace Eden.Test
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ParticleBomb : IParticleEmitter
    {

        [SerializeField] private int _particleCount = 100;
        [SerializeField] private Particle _particlePrefab = null;
        [SerializeField] private float _deceleration = 10.0f;
        [SerializeField] private float _speedMin = 600.0f;

        [SerializeField] private float _speedMax = 900.0f;
        [SerializeField] private Color _startColor = Color.yellow;
        [SerializeField] private Color _endColor = Color.red;

        private Particle[] _liveParticles;
        private int alive;


        public void set_particlePrefab(Particle pp)

        {
            _particlePrefab = pp;
        }

        public override int ParticleCount { get { return alive; } }

        public void decreaseAlive() { alive--; }

        public Particle[] LiveParticles { get { return _liveParticles; } }

        public void Explode()
        {
            alive = _particleCount;
            _liveParticles = new Particle[_particleCount];

            for (int i = 0; i < _particleCount; i++)
            {
                Vector3 v = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
                _liveParticles[i] = Instantiate(_particlePrefab);
                _liveParticles[i].Init(2.0f, 1.0f, Color.Lerp(_startColor, _endColor, Random.value), v * Random.Range(_speedMin, _speedMax), _deceleration);
                _liveParticles[i].transform.SetParent(transform, false);
                //_liveParticles.Add(p);
                //GameParticule.addParticule(p);
            }
        }

        public override void Delete50()
        {
            for (int i = 0; i < LiveParticles.Length; i++)
            {
                if (LiveParticles[i].Lifespan > 0 && Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    LiveParticles[i].Lifespan = 0.0f;
                    decreaseAlive();
                }
            }
        }
    }
}