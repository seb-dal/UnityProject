using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSParticle : MonoBehaviour
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

    public float ColorChangeDuration { get { return _colorChangeDuration; } set { _colorChangeDuration = value; } }
    public SpriteRenderer Sprite { get { return sprite; } }
    public float Lifespan { get { return _lifespan; } set { _lifespan = value; } }
    public bool ChangeColor { get { return _changeColor; } }

    #endregion Properties


    #region Methods
    public void Start() { sprite = GetComponentInChildren<SpriteRenderer>(); }

    public void Init(float lifespan, float fade, Color color, Vector3 initialSpeed, float deceleration, bool changeColor = false)
    {
        gameObject.transform.localPosition = Vector3.zero;
        _lifespanMax = _lifespan = lifespan;
        sprite.color = color;
        _previousColor = color;
        _colorChangeDuration = UnityEngine.Random.Range(_colorChangeDurationMin, _colorChangeDurationMax);
        _speed = initialSpeed;
        _deceleration = deceleration;
        _changeColor = changeColor;
        _fadeDurationInit = _fadeDuration = fade;

    }

    // sprite.enabled seems faster than gameObject.SetActive();
    public void Show() { if (sprite != null) sprite.enabled = (true); }

    public void Hide() { if (sprite != null) sprite.enabled = (false); }

    public bool IsDead() { return _lifespan <= 0.0f; }

    #endregion Methods

    /**
     * insted of letting each particule executing the same update, we use a computeShader to make the GPU
     * do all the heavy computation and we just let the CPU do the get and set of the values.
     * 
     */
}

/*
 Data Structure of a Particle
    - replace Color by Vector4 (same)
 */
public struct ParticuleShader
{
    public Vector3 _speed;
    public Vector3 _position;

    public float _lifespanMax;
    public float _lifespan;

    public float _fadeDurationInit;
    public float _fadeDuration;

    public float _colorChangeTime;
    public float _colorChangeDuration;

    public Vector4 _previousColor;
    public Vector4 _nextColor;
    public Vector4 _spriteColor;

    public float _deceleration;
    public int _changeColor;
};