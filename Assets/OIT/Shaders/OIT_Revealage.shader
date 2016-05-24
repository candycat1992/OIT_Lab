Shader "Custom/OIT Revealage" {
	Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Revealage"}
		
		Pass {
			ZWrite Off
			Blend Zero OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			sampler2D _MainTex;
			
			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD1;
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			float4 frag(v2f i) : SV_Target {
				fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
				return albedo.aaaa;
			}
			
			ENDCG
		}
	}
}
