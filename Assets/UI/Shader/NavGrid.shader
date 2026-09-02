// Square navigation grid for the cockpit nav pane.
// Drop-in replacement for GridSpace.shadergraph: it declares the same property
// names UIController already writes (_ShipLocV2, _ShipRotation, _CellSize,
// _GridAmmount, _GridThickness, _WorldView), so no C# change is needed —
// only the `gridSha` reference on UIController.
//
// Unlike GridSpace it has no ellipse mask: the grid runs edge to edge.
Shader "Starwolf/NavGrid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Palette)]
        _BackgroundColor ("Background", Color) = (0.024, 0.055, 0.075, 1)
        _GridColor ("Minor Line", Color) = (0.325, 0.85, 0.78, 0.30)
        _MajorColor ("Major Line", Color) = (0.45, 1, 0.87, 0.62)
        _AxisColor ("Centre Axis", Color) = (0.95, 0.72, 0.22, 0.85)

        [Header(Grid)]
        // Written by UIController.WorldGridLocUpdate / RotUpdate / Zoom.
        _ShipLocV2 ("Ship Location", Vector) = (0, 0, 0, 0)
        _ShipRotation ("Ship Rotation (degrees)", Float) = 0
        _CellSize ("Cell Size", Vector) = (1, 1, 0, 0)
        _GridAmmount ("Grid Amount (cells across)", Float) = 5
        _GridThickness ("Line Thickness (fraction of pane)", Float) = 0.02
        _WorldView ("World View (0 = ship up, 1 = north up)", Float) = 0

        [Header(Styling)]
        _MajorEvery ("Major Line Every N Cells", Float) = 5
        _MajorScale ("Major Line Width Scale", Float) = 2
        _AxisEnable ("Centre Crosshair", Float) = 1
        _Aspect ("Aspect (width / height)", Float) = 1

        [Header(UI Stencil)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "NavGrid"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half4  _BackgroundColor;
                half4  _GridColor;
                half4  _MajorColor;
                half4  _AxisColor;
                float4 _ShipLocV2;
                float4 _CellSize;
                float  _ShipRotation;
                float  _GridAmmount;
                float  _GridThickness;
                float  _WorldView;
                float  _MajorEvery;
                float  _MajorScale;
                float  _AxisEnable;
                float  _Aspect;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            // Screen-derivative antialiased line mask. `g` is in cell units, so a
            // line sits on every integer; `halfWidth` is also in cell units.
            float GridLine(float2 g, float halfWidth)
            {
                float2 d = abs(frac(g + 0.5) - 0.5);
                float2 w = fwidth(g) * 0.5 + 1e-5;
                float2 a = 1.0 - smoothstep(halfWidth - w, halfWidth + w, d);
                return max(a.x, a.y);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Pane space: origin at the ship, +x right, +y up, edges at +/-0.5.
                float2 pane = IN.uv - 0.5;
                pane.x *= _Aspect;

                // Ship-up view counter-rotates the plot so the bow always points north
                // on the display. World view leaves it fixed.
                float2 p = pane;
                if (_WorldView < 0.5)
                {
                    float s, c;
                    sincos(radians(_ShipRotation), s, c);
                    p = float2(p.x * c - p.y * s, p.x * s + p.y * c);
                }

                // Pane -> world, then world -> cell units.
                float2 cell  = max(abs(_CellSize.xy), 1e-4);
                float2 world = p / cell + _ShipLocV2.xy;
                float2 g     = world * _GridAmmount;

                // _GridThickness is a fraction of the pane; convert to a half-width in cells.
                float halfMinor = max(_GridThickness * _GridAmmount * 0.5, 1e-4);
                float majorEvery = max(_MajorEvery, 1.0);
                float halfMajor  = halfMinor * max(_MajorScale, 1.0) / majorEvery;

                float majorMask = GridLine(g / majorEvery, halfMajor);
                float minorMask = saturate(GridLine(g, halfMinor) - majorMask);

                // Crosshair stays locked to the pane, not to the rotating plot.
                float halfAxis = _GridThickness * 0.5 * max(_MajorScale, 1.0);
                float2 cw = fwidth(pane) + 1e-5;
                float2 axis2 = 1.0 - smoothstep(halfAxis - cw, halfAxis + cw, abs(pane));
                float axisMask = max(axis2.x, axis2.y) * step(0.5, _AxisEnable);

                half4 col = _BackgroundColor;
                col.rgb = lerp(col.rgb, _GridColor.rgb,  minorMask * _GridColor.a);
                col.a   = max(col.a,                     minorMask * _GridColor.a);
                col.rgb = lerp(col.rgb, _MajorColor.rgb, majorMask * _MajorColor.a);
                col.a   = max(col.a,                     majorMask * _MajorColor.a);
                col.rgb = lerp(col.rgb, _AxisColor.rgb,  axisMask  * _AxisColor.a);
                col.a   = max(col.a,                     axisMask  * _AxisColor.a);

                col *= _Color * IN.color;
                col.a *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;
                return col;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
