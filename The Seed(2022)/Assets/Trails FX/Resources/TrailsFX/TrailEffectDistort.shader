Shader "TrailsFX/Effect/Distort" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _Cull ("Cull", Int) = 2
    _AdditiveTint ("Additive Tint", Color) = (0,0,0.1)
}

        SubShader
    {
        Tags { "Queue"="Transparent+101" "RenderType"="Transparent" "RenderPipeline" = "LightweightPipeline" }

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

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing assumeuniformscaling nolightprobe nolodfade nolightmap
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos     : SV_POSITION;
                float4 grabPos : TEXCOORD1;
                fixed4 color   : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

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
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.color = UNITY_ACCESS_INSTANCED_PROP(_Colors_arr, _Colors);
                o.grabPos.xy += (0.5.xx - o.color.rg) * o.color.a / o.grabPos.w;
                return o;
            }

            sampler2D _CameraOpaqueTexture;
            fixed4 _AdditiveTint;

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2Dproj(_CameraOpaqueTexture, i.grabPos);
                col.rgb += _AdditiveTint;
                col.a *= i.color.a;
                return col;
            }
            ENDCG
        }

    }

    SubShader
    {
        Tags { "Queue"="Transparent+101" "RenderType"="Transparent" }

        GrabPass {
        "_BackgroundTexture"
        }

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
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
            	UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos     : SV_POSITION;
                float4 grabPos : TEXCOORD1;
                fixed4 color   : COLOR;
				UNITY_VERTEX_OUTPUT_STEREO
            };

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
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.color = UNITY_ACCESS_INSTANCED_PROP(_Colors_arr, _Colors);
                o.grabPos.xy += (0.5.xx - o.color.rg) * o.color.a / o.grabPos.w;
                return o;
            }

            sampler2D _BackgroundTexture;
            fixed4 _AdditiveTint;

            fixed4 frag (v2f i) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(i);
            	fixed4 col = tex2Dproj(_BackgroundTexture, i.grabPos);
                col.rgb += _AdditiveTint;
            	col.a *= i.color.a;
				return col;
            }
            ENDCG
        }

    }


}