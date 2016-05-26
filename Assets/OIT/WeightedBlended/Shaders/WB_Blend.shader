Shader "Hidden/OIT/Weighted Blended/Final Blend" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
		_AccumTex ("Accum", 2D) = "black" {}
		_RevealageTex ("Revealage", 2D) = "white" {}
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		
		Pass {
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			sampler2D _AccumTex;
			sampler2D _RevealageTex;
			
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
			
			fixed4 frag(v2f i) : SV_Target {
				fixed4 background = tex2D(_MainTex, i.uv);
				float4 accum = tex2D(_AccumTex, i.uv);
				float revealage = tex2D(_RevealageTex, i.uv).r;
				fixed4 col = float4(accum.rgb / clamp(accum.a, 1e-4, 5e4), revealage);
				return (1.0 - col.a) * col + col.a * background;
			}
			
			ENDCG
		}
	} 
	FallBack "Transparent/VertexLit"
}
