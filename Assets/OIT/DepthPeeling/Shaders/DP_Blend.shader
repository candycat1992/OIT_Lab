Shader "Hidden/OIT/Depth Peeling/Final Blended" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }

		Pass {
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;

			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert(a2v v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target {
				return fixed4(1, 1, 1, 1);
			}
			
			ENDCG
		}
		
		Pass {			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			sampler2D _LayerTex;
			
			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert(a2v v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;			
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 layer = tex2D(_LayerTex, i.uv);
				return layer.a * layer + (1 - layer.a) * col;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse/VertexLit"
}
