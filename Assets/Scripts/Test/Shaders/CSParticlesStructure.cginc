struct ParticuleShader
{
    float3 _speed;
    float4x4 mat;
	
    
    float _lifespanMax;
    float _lifespan;

    float _fadeDurationInit;
    float _fadeDuration;
	
    
    float _colorChangeTime;
    float _colorChangeDuration;
	
    float4 _previousColor;
    float4 _nextColor;
    float4 _spriteColor;
	
    float _deceleration;
    
    int _changeColor;
};