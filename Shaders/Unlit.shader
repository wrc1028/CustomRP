Shader "CustomRP/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha Test On", float) = 0
        _Cutout ("Cutout", Range(0, 1)) = 0.5

        
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("__src", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("__dst", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend [_SrcBlend][_DstBlend]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile _ _ALPHATEST_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutout;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                clip(col.a - _Cutout);
                return col;
            }
            ENDCG
        }
    }
}
