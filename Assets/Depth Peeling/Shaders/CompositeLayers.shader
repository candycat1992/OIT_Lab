Shader "Hidden/Depth Peel/Composite" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_ColorBuffer ("Color Buffer", 2D) = "black" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	sampler2D _ColorBuffer;
	float4 _MainTex_TexelSize;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		
		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy;
		#if SHADER_API_D3D9
		if (_MainTex_TexelSize.y < 0)
			o.uv[0].y = 1-o.uv[0].y;
		#endif			
		return o;
	}
	
	half4 fragComposite(v2f i) : COLOR 
	{
		half4 color = tex2D (_MainTex, i.uv[1]);
		half4 buffer = tex2D (_ColorBuffer, (i.uv[0]));
		return color * (1 - buffer.a) + buffer * buffer.a; 
	}	

	ENDCG
	
Subshader {
  Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment fragComposite
      ENDCG
  } 
}

Fallback off
	
} // shader