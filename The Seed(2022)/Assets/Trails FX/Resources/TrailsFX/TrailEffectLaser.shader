Shader "TrailsFX/Effect/Laser" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _LaserData("Laser Data", Vector) = (0.1, 20, 0.1)
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
            #include "UnityCG.cginc"
            #pragma target 3.5

            struct appdata
            {
                float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
            	UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos    : SV_POSITION;
                float3 vertex : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
            };

UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(fixed4, _Colors)
#define _Colors_arr Props
UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v, uint vid : SV_VertexID)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.vertex = v.vertex;
                return o;
            }

            float3 _LaserData;

            fixed4 frag (v2f i) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(i);
              	fixed4 col = UNITY_ACCESS_INSTANCED_PROP(_Colors_arr, _Colors);
              	float seq = (abs(i.vertex.y) + _LaserData.x * 0.5) % _LaserData.x;
              	seq = 1.0 - saturate(abs(seq - _LaserData.x * 0.5) * _LaserData.y);
              	seq *= (col.a % _LaserData.z)> (_LaserData.z * 0.5);
              	col *= seq;
				return col;
            }
            ENDCG
        }

    }
}