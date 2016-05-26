Shader "Hidden/Depth Peel/Color Buffer" {
	Properties {
		_MainTex ("Main Texture", 2D) = "" {}
		_BumpMap ("Bump Map", 2D) = "bump" {}
		_Color ("Color", COLOR) = (0.75, 0.75, 0.75, 1)
	}
    SubShader {
		Tags { "RenderType" = "" }
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert noshadow
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 screenPos;
			float depth;
		};
      
      	sampler2D _BumpMap;
		sampler2D _DepthTex;
      	sampler2D _MainTex;
      	float4 _Color;
      
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o)
			o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w;
		}
		    
		void surf (Input IN, inout SurfaceOutput o) {
			half4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
			
			float depth = IN.depth;
			float prevDepth = DecodeFloatRGBA(tex2Dproj(_DepthTex, UNITY_PROJ_COORD(IN.screenPos)));
			
			// need slight bias
			clip(depth - (prevDepth - 0.00001));
			o.Albedo = mainTex.rgb * _Color.rgb;
			o.Alpha = _Color.a;    
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
      ENDCG
    }
    Fallback "Diffuse"
  }

