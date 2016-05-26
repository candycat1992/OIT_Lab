Shader "Hidden/Depth Peel/Render" {
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float4 screenPos: TEXCOORD0;
		float depth :TEXCOORD1;
	};
	
	sampler2D _DepthTex;
	
		
	v2f vert(appdata_base v) {
		v2f o;
		
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.screenPos = ComputeScreenPos(o.pos);
		o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z;
		
		return o;
	}
	
	half4 frag (v2f i) : COLOR {
		float depth = i.depth * _ProjectionParams.w;
		float prevDepth = DecodeFloatRGBA(tex2Dproj(_DepthTex, UNITY_PROJ_COORD(i.screenPos)));
		
		clip(depth - (prevDepth + 0.00001));
		
		return EncodeFloatRGBA(depth);
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

	
} // shader



