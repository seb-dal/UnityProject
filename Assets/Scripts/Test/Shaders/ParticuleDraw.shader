Shader "Custom/ParticuleDraw" {
	SubShader{
		// Alpha blending
		Tags {
			"Queue" = "AlphaTest"
			"RenderType" = "TransparentCutout"
			"IgnoreProjector" = "True"
		}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off // Draw from both side

		Pass {
			CGPROGRAM

			#pragma vertex vertexShader
			#pragma fragment fragmentShader

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex   : POSITION; // vertex position
				float4 color    : COLOR;  // Color
				float2 uv : TEXCOORD0; // texture coordinate
			};

			struct v2f {
				float2 uv : TEXCOORD0; // texture coordinate
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;

				float _lifespan : LIFESPAN;
			};

			struct ParticuleShader
			{
				// Only mat and color used.
				// same structure format between computeShader and Shader to simplify setting buffer
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

			// Buffer of Particles
			StructuredBuffer<ParticuleShader> _Particules;

			v2f vertexShader(appdata_t i, uint instanceID: SV_InstanceID) {
				v2f o;

				float4 pos = mul(_Particules[instanceID].mat, i.vertex);
				o.vertex = UnityObjectToClipPos(pos);
				o.color = _Particules[instanceID]._spriteColor;

				o.uv = i.uv;
				o._lifespan = _Particules[instanceID]._lifespan;
				return o;
			}

			sampler2D _MainTex;

			fixed4 fragmentShader(v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				if (col.a < 0.3f) // discard low alpha value
					discard;
				if (i.color.a < 0.0f)
					discard;
				if (i._lifespan <= 0.0f)
					discard;

				return col * i.color;
			}

			ENDCG
		}
	}
}

