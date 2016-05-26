Shader "Hidden/Render Depth" {
	Properties {
		_MainTex ("Main Texture", 2D) = "" {}		
	}
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float depth :TEXCOORD0;
	};
	
	sampler2D _MainTex;
	
	v2f vert( appdata_base v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);		
		o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w;
		return o;
	}
	
	float4 frag (v2f i) : COLOR {
		return EncodeFloatRGBA(i.depth);
	}
	ENDCG 
	
Subshader {
	Fog { Mode off }  
	Pass {
		CGPROGRAM  
		#pragma vertex vert
		#pragma fragment frag
		ENDCG
	}
}
	
}