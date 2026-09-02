Shader "Starwolf/TacticalGrid"
{
    // A world-space grid for the tactical plot.
    // Lines come from world XY, so panning and zooming move across the grid
    // rather than scaling it.
    //
    // Three tiers: fine, minor, major. Each tier fades out on its own once its
    // lines crowd closer than a few pixels apart, so zooming out thins the grid
    // instead of turning it into mush, and zooming in reveals finer detail.
    Properties
    {
        _BackgroundColor ("Background",   Color) = (0.055, 0.043, 0.031, 1)
        _FineColor       ("Fine Line",    Color) = (0.816, 0.608, 0.204, 0.05)
        _GridColor       ("Minor Line",   Color) = (0.816, 0.608, 0.204, 0.10)
        _MajorColor      ("Major Line",   Color) = (0.816, 0.608, 0.204, 0.26)
        _AxisColor       ("Centre Axis",  Color) = (0.910, 0.639, 0.239, 0.55)

        _CellSize        ("Cell Size (world units)", Float) = 2000
        _FineDivisions   ("Fine Lines Per Cell", Float) = 5
        _MajorEvery      ("Major Line Every N Cells", Float) = 5
        _LineWidth       ("Line Width (PIXELS)", Float) = 1.1
        _FineWidthScale  ("Fine Line Width Scale", Float) = 0.6
        _MajorWidthScale ("Major Line Width Scale", Float) = 2.2
        _AxisWidthScale  ("Axis Width Scale", Float) = 2.6
        _AxisEnable      ("Centre Crosshair", Float) = 0

        // A tier is gone below _FadeMin pixels of separation and full above _FadeMax.
        _FadeMin         ("Fade Out Below (px)", Float) = 6
        _FadeMax         ("Full Above (px)", Float) = 26
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f
            {
                float4 pos   : SV_POSITION;
                float3 world : TEXCOORD0;
            };

            fixed4 _BackgroundColor, _FineColor, _GridColor, _MajorColor, _AxisColor;
            float  _CellSize, _FineDivisions, _MajorEvery, _LineWidth;
            float  _FineWidthScale, _MajorWidthScale, _AxisWidthScale, _AxisEnable;
            float  _FadeMin, _FadeMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Coverage of a line set along one axis, 0..1.
            // Width is given in PIXELS. fwidth(coord) is world units per pixel,
            // so multiplying by it holds the stroke at a constant thickness on
            // screen at any zoom. A world-unit width would balloon as you zoom in.
            float lineCoverage (float coord, float spacing, float widthPx)
            {
                float perPixel = max(fwidth(coord), 1e-9);
                float d = abs(frac(coord / spacing + 0.5) - 0.5) * spacing;
                float half = widthPx * 0.5 * perPixel;
                return 1.0 - smoothstep(half - perPixel * 0.5, half + perPixel * 0.5, d);
            }

            // How far apart this tier's lines are, in pixels.
            float pixelSpacing (float2 p, float spacing)
            {
                float perPixel = max(fwidth(p.x), fwidth(p.y));
                return spacing / max(perPixel, 1e-6);
            }

            // One tier: coverage in both axes, scaled by how crowded it is.
            float tier (float2 p, float spacing, float width)
            {
                float cov = max(lineCoverage(p.x, spacing, width),
                                lineCoverage(p.y, spacing, width));
                float fade = smoothstep(_FadeMin, _FadeMax, pixelSpacing(p, spacing));
                return cov * fade;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.world.xy;

                float minorSpacing = max(_CellSize, 0.0001);
                float fineSpacing  = minorSpacing / max(_FineDivisions, 1);
                float majorSpacing = minorSpacing * max(_MajorEvery, 1);

                float fine  = tier(p, fineSpacing,  _LineWidth * _FineWidthScale);
                float minor = tier(p, minorSpacing, _LineWidth);
                float major = tier(p, majorSpacing, _LineWidth * _MajorWidthScale);

                fixed4 col = _BackgroundColor;
                col = lerp(col, _FineColor,  fine  * _FineColor.a);
                col = lerp(col, _GridColor,  minor * _GridColor.a);
                col = lerp(col, _MajorColor, major * _MajorColor.a);

                // Centre crosshair: the world origin, not the ship. Off by default.
                if (_AxisEnable > 0.5)
                {
                    float ppx = max(fwidth(p.x), 1e-9), ppy = max(fwidth(p.y), 1e-9);
                    float hx = _LineWidth * _AxisWidthScale * 0.5 * ppx;
                    float hy = _LineWidth * _AxisWidthScale * 0.5 * ppy;
                    float axis = max(
                        1.0 - smoothstep(hx - ppx * 0.5, hx + ppx * 0.5, abs(p.x)),
                        1.0 - smoothstep(hy - ppy * 0.5, hy + ppy * 0.5, abs(p.y)));
                    col = lerp(col, _AxisColor, axis * _AxisColor.a);
                }

                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
