Shader "TrailsFX/Effect/Clone" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _CutOff("Cut Off", Float) = 0.5
    _Cull ("Cull", Int) = 2
}
    SubShader
    {
        Tags { "Queue"="Transparent+101" "RenderType"="Transparent" }

        Pass
        {
			Stencil {
                Ref 2
                ReadMask 2
                Comp NotEqual
                Pass replace
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing assumeuniformscaling nolightprobe nolodfade nolightmap
            #pragma multi_compile _ TRAIL_ALPHACLIP
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
            	UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
            };

      		sampler2D _MainTex;
      		float4 _MainTex_ST;
      		fixed _CutOff;

UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(fixed4, _Colors)
#define _Colors_arr Props
UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            inline fixed getLuma(fixed3 rgb) {
				const fixed3 lum = float3(0.299, 0.587, 0.114);
				return dot(rgb, lum);
			}


            fixed4 frag (v2f i) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv);
                #if TRAIL_ALPHACLIP
              	clip(getLuma(col.rgb) - _CutOff);
              	#endif
              	col *= UNITY_ACCESS_INSTANCED_PROP(_Colors_arr, _Colors);
				return col;
            }
            ENDCG
        }

    }
}