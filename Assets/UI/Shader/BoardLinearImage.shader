// A live camera image (linear colour) shown on the handoff board, which stores sRGB values and
// converts them once at the end (BoardGammaToLinear). This encodes the image to sRGB first, so it
// comes out as rendered. Keeps the plot's RectMask2D clipping. Used for our ship's 3D view, which the
// VectorModel full-screen pass draws as lines on black: black is made see-through.
Shader "Starwolf/UI/BoardLinearImage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Image", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle] _KeyBlack ("Black is see-through (line-art views)", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _KeyBlack;
            float4 _ClipRect;

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 world : TEXCOORD1; };

            v2f vert (appdata v)
            {
                v2f o;
                o.world = v.vertex;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                c.rgb = LinearToGammaSpace(saturate(c.rgb));
                // the VectorModel full-screen pass draws lines on solid black; let the black go
                if (_KeyBlack > 0.5) c.a *= saturate(max(c.r, max(c.g, c.b)));
                c *= i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(i.world.xy, _ClipRect);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
