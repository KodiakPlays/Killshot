// The close-up's glow (owner 2026-11-08: a shot fired looks like a shot): shot streaks, sparks and blasts. It adds light:
// the picture's see-through parts are its shape, and the colour can go above 1, where the close-up's glow (bloom) picks it up.
// It writes no see-through value of its own, so the plot shows the close-up's picture with its light added over the nebula.
Shader "Starwolf/Close Up Glow"
{
    Properties
    {
        _MainTex ("Shape (its see-through parts)", 2D) = "white" {}
        [HDR] _Color ("Colour", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;

            struct appdata { float4 vertex : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half shape = tex2D(_MainTex, i.uv).a * i.color.a;
                return half4(i.color.rgb * shape, 0);
            }
            ENDCG
        }
    }
}
