namespace Eden.Test
{
    using System.Collections;
    using UnityEngine;

    public class BombSpawnerGPU : MonoBehaviour
    {
        [SerializeField] private float _spawnIntervalMin = 1.0f;
        [SerializeField] private float _spawnIntervalMax = 4.0f;
        [SerializeField] private ParticleBombGPU _bombPrefab = null;

        // add prefab of particle to have circlular particle
        [SerializeField] private GameObject _particlePrefab = null;
        [SerializeField] private Transform _bombAnchor = null;

        private void Start()
        {
            float delay = Random.Range(_spawnIntervalMin, _spawnIntervalMax);
            StartCoroutine(SpawnNextBomb(delay));
        }

        private IEnumerator SpawnNextBomb(float delay)
        {
            yield return new WaitForSeconds(delay);

            Vector3 position = new Vector3(
                Random.Range(-50.0f, 50.0f),
                Random.Range(-50.0f, 50.0f),
                0.0f);

            ParticleBombGPU bomb = Instantiate(_bombPrefab);

            // Transfere the particle prefab to ParticleBomb
            bomb.Set_particlePrefab(_particlePrefab);
            bomb.Initialize();
            bomb.transform.SetParent(_bombAnchor, false);
            bomb.transform.position = position;
            bomb.Explode();

            // destroy automatically the bomb object after 2.5 seconds
            // prevent the accumulation of useless bomb
            Destroy(bomb.gameObject, 2.5f);

            float newDelay = Random.Range(_spawnIntervalMin, _spawnIntervalMax);
            StartCoroutine(SpawnNextBomb(newDelay));
        }
    }
}