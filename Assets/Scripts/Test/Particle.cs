namespace Eden.Test
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class Particle : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float _colorChangeDurationMin = 0.3f;
        [SerializeField] private float _colorChangeDurationMax = 1.5f;

        private Vector3 _speed = Vector3.zero;

        private float _lifespanMax = 0.0f;
        private float _lifespan = 0.0f;

        private float _fadeDurationInit = 0.0f;
        private float _fadeDuration = 0.0f;

        private float _colorChangeTime = 0.0f;
        private float _colorChangeDuration = 1.0f;
        private Color _previousColor = Color.white;
        private Color _nextColor = Color.white;
        private float _deceleration = 0.0f;
        private bool _changeColor = false;

        private SpriteRenderer sprite;
        #endregion Fields

        #region Properties
        public float Lifespan { get { return _lifespan; } set { _lifespan = value; } }
        #endregion Properties

        #region Methods

        public void Start() { sprite = GetComponentInChildren<SpriteRenderer>(); }


        public void Init(float lifespan, float fade, Color color, Vector3 initialSpeed, float deceleration, bool changeColor = false)
        {
            gameObject.transform.localPosition = Vector3.zero;
            _lifespanMax = _lifespan = lifespan;
            if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
            sprite.color = _previousColor = color;
            _colorChangeDuration = UnityEngine.Random.Range(_colorChangeDurationMin, _colorChangeDurationMax);
            _speed = initialSpeed;
            _deceleration = deceleration;
            _changeColor = changeColor;

            _fadeDurationInit = _fadeDuration = fade;
        }


        public void Show() { if (sprite != null) sprite.enabled = (true); }

        public void Hide() { if (sprite != null) sprite.enabled = (false); }

        public bool IsDead() { return _lifespan <= 0.0f; }


        private void Update()
        {
            if (IsDead())
            {
                Hide();
                return;
            }

            _lifespan -= Time.deltaTime;
            _fadeDuration -= Time.deltaTime;
            transform.position = transform.position + _speed * Time.deltaTime;
            _speed -= _speed * _deceleration * Time.deltaTime;

            UpdateColor();
        }



        private void UpdateColor()
        {
            Color a = new Color(0, 0, 0, Mathf.Max(-_fadeDuration, 0.0f) / (_lifespanMax - _fadeDurationInit));
            if (!_changeColor)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1) - a;
                return;
            }

            if (_colorChangeTime > _colorChangeDuration)
            {
                _previousColor = _nextColor;
                _nextColor = UnityEngine.Random.ColorHSV();
                _colorChangeTime -= _colorChangeDuration;
            }


            sprite.color = Color.Lerp(_previousColor, _nextColor, _colorChangeTime / _colorChangeDuration) - a;
            _colorChangeTime += Time.deltaTime;
        }
        #endregion Methods
    }
}