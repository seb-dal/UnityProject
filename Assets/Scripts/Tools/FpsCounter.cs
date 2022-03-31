namespace Eden.Tools
{
	using UnityEngine;

	public class FpsCounter : MonoBehaviour
	{
		#region Fields
		[SerializeField] private float _refreshInterval = 1.0f;

		private float _time = 0.0f;
		private int _frameCount = 0;
		private int _fps = 0;
		#endregion Fields

		#region Properties
		public int Fps { get { return _fps; } }
		#endregion Properties

		#region Methods
		private void Start()
		{
			_time = _refreshInterval;
		}

		private void Update()
		{
			_frameCount++;

			_time += Time.deltaTime;

			if (_time >= _refreshInterval)
			{
				_fps = Mathf.CeilToInt(_frameCount / _time);
				_frameCount = 0;
				_time -= _refreshInterval;
			}
		}
		#endregion Methods
	}
}