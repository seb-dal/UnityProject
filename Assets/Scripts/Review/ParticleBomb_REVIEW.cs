namespace Eden.Review
{
    using System.Collections.Generic;
    using Eden.Test;
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

        public List<Particle> _liveParticles = new List<Particle>();

        public override int ParticleCount { get { return _liveParticles.Count; } }

        public void Explode()
        {
            for (int i = 0; i < _particleCount; i++)
            {
                Vector3 v = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
                Particle p = Instantiate(_particlePrefab);
                p.Init(2.0f, 1.0f, Color.Lerp(_startColor, _endColor, Random.value), v * Random.Range(_speedMin, _speedMax), _deceleration);
                p.transform.SetParent(transform, false);
                _liveParticles.Add(p);
            }
        }

        public void Update()
        {
            for (int i = 0; i < _liveParticles.Count; i++)
            {
                if (_liveParticles[i].IsDead())
                {
                    Destroy(_liveParticles[i].gameObject);
                    _liveParticles.RemoveAt(i--);
                }
            }
        }

        public override void Delete50()
        {

        }
    }
}