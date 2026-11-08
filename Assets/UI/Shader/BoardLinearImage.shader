// A live camera image (linear colour) shown on the handoff board, which stores sRGB values and
// converts them once at the end (BoardGammaToLinear). This encodes the image to sRGB first, so it
// comes out as rendered. Keeps the plot's RectMask2D clipping. Used for our ship's 3D view, which the
// VectorModel full-screen pass draws as lines on black: black is made see-through and the lines
// take the Tint (the image's colour), so our ship can be white while the effect itself stays red.
Shader "Starwolf/UI/BoardLinearImage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Image", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle] _KeyBlack ("Line art: black see-through, lines take the Tint", Float) = 1
        // Owner 2026-11-08: the close-up sets 1 (One). Its picture's light then adds over what is behind it, so a glow or a
        // spark over empty space shows. Every other image keeps 5 (SrcAlpha).
        [HideInInspector] _SrcBlend ("Blend", Float) = 5
        // Step 109: the UI Mask's stencil, so the periscope's round sight can clip the Garuda's picture.
        // Outside a Mask these stay at their defaults and change nothing.
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] OneMinusSrcAlpha
        ColorMask [_ColorMask]

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
            float _KeyBlack, _SrcBlend;
            float4 _ClipRect;
            // owner 2026-11-02: the plot's sight circles (PlotPanel.ClipToSight), centre xy and radius z in canvas space. With
            // _CircleCount 0 (every other image) nothing is cut; else what is outside them goes, or with _KeepOutside 1 what is inside.
            float4 _Circles[4];
            float _CircleCount, _KeepOutside;

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
                // the VectorModel full-screen pass draws lines on solid black; let the black go and
                // use the lines as a mask for the tint
                if (_KeyBlack > 0.5) { c.a *= saturate(max(c.r, max(c.g, c.b))); c.rgb = 1; }
                c *= i.color;
                float keep = 1;
                if (_CircleCount > 0)
                {
                    float inside = 0;
                    for (int k = 0; k < 4; k++) if (k < _CircleCount && distance(i.world.xy, _Circles[k].xy) <= _Circles[k].z) inside = 1;
                    keep = _KeepOutside > 0.5 ? 1 - inside : inside;
                }
                #ifdef UNITY_UI_CLIP_RECT
                keep *= UnityGet2DClipping(i.world.xy, _ClipRect);
                #endif
                c.a *= keep;
                if (_SrcBlend < 1.5) c.rgb *= i.color.a * keep; // Blend One: the picture's colour is already weighted by its own see-through value
                return c;
            }
            ENDCG
        }
    }
}
