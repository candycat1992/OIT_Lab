Shader "Hidden/OIT/Depth Peeling/Initialization" {
	Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
	}
	SubShader {
		Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Transparent"}
		
		Pass {
			Tags { "LightMode"="ForwardBase" }

			ZWrite On
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Lighting.cginc"
			
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			
			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float3 lightDir: TEXCOORD0;
				float3 viewDir : TEXCOORD1;
				float2 uv : TEXCOORD2;
				float depth : TEXCOORD3;
			};

			struct PixelOutput {
				fixed4 col : COLOR0;
				fixed4 depth : COLOR1;
			};
			
			v2f vert(a2v v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				TANGENT_SPACE_ROTATION;
				// Transform the light direction from object space to tangent space
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
				// Transform the view direction from object space to tangent space
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

				o.depth = COMPUTE_DEPTH_01;
				
				return o;
			}
			
			PixelOutput frag(v2f i) : SV_Target {
				fixed3 tangentLightDir = normalize(i.lightDir);
				fixed3 tangentViewDir = normalize(i.viewDir);
				fixed3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv));

				fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(tangentNormal, tangentLightDir));

				PixelOutput o;
				o.col = fixed4(ambient + diffuse, _Color.a);
				o.depth = EncodeFloatRGBA(i.depth);
				return o;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse/VertexLit"
}
