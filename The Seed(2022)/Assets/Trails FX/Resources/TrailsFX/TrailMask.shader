Shader "TrailsFX/Mask" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("Cut Off", Float) = 0.5
    _Cull ("Cull", Int) = 2
}
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }

        // Create mask
        Pass
        {
			Stencil {
                Ref 2
                WriteMask 2
                Comp always
                Pass replace
            }
            ColorMask 0
            ZTest Always
            ZWrite Off
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos: SV_POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
            };

      		sampler2D _MainTex;
      		fixed _CutOff;

            v2f vert (appdata v)
            {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 col = tex2D(_MainTex, i.uv);
            	#if TRAIL_ALPHACLIP
            	clip(col.a - _CutOff);
            	#endif
            	return 0;
            }
            ENDCG
        }

    }
}