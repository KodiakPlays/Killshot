// Shows the handoff board's render texture on screen. The board is drawn into a linear-format
// texture with undecoded sprites and gamma vertex colours, so its translucency blends the way a
// browser blends it. Those stored values are sRGB; this converts them once, so the sRGB backbuffer
// shows them unchanged. See GammaBoardPresenter.cs.
Shader "Starwolf/UI/BoardGammaToLinear"
{
    Properties
    {
        [PerRendererData] _MainTex ("Board", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 c = tex2D(_MainTex, i.uv).rgb;
                return fixed4(GammaToLinearSpace(c), 1);
            }
            ENDCG
        }
    }
}
