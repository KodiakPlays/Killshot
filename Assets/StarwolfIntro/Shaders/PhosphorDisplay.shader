Shader "Starwolf/PhosphorDisplay"
{
    Properties
    {
        _Ident ("Section 36", 2D) = "black" {}
        _Boot ("Initialising", 2D) = "black" {}
        _Title ("Starwolf", 2D) = "black" {}
        _NewGame ("New Game", 2D) = "black" {}
        _LoadGame ("Load Game", 2D) = "black" {}
        _Settings ("Settings", 2D) = "black" {}
        // Everything below is written from StarwolfIntroController. It must be declared here:
        // a uniform the Properties block does not name is dropped whenever the shader reloads,
        // and the intro then draws a black screen until the next play.
        _IdentSize ("Ident size", Vector) = (0,0,0,0)
        _BootSize ("Boot size", Vector) = (0,0,0,0)
        _TitleSize ("Title size", Vector) = (0,0,0,0)
        _NewGameSize ("New game size", Vector) = (0,0,0,0)
        _LoadGameSize ("Load game size", Vector) = (0,0,0,0)
        _SettingsSize ("Settings size", Vector) = (0,0,0,0)
        _Phosphor ("Phosphor", Color) = (0,0,0,1)
        _Background ("Background", Color) = (0,0,0,1)
        _Pitch ("Cell pitch", Float) = 9
        _Stage ("Stage", Float) = 0
        _Phase ("Phase", Float) = 0
        _Fade ("Fade", Float) = 0
        _Clock ("Clock", Float) = 0
        _ZoomProgress ("Zoom progress", Float) = 0
        _Selected ("Selected", Float) = 0
        _LoadAvailable ("Load available", Float) = 0
        _GridVisibility ("Grid visibility", Float) = 0
        // CRT treatment. Tune these to taste; nothing in code writes them.
        _ScanDepth ("Scanline depth", Range(0,1)) = 0.18
        _FieldGlow ("Unlit field glow", Range(0,0.05)) = 0.006
        _Vignette ("Vignette", Range(0,2)) = 0.45
        _ScreenInset ("Screen edge falloff", Range(0.001,0.3)) = 0.06
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _Ident, _Boot, _Title, _NewGame, _LoadGame, _Settings;
            float4 _IdentSize, _BootSize, _TitleSize, _NewGameSize, _LoadGameSize, _SettingsSize;
            float4 _Phosphor, _Background;
            float _Stage, _Phase, _Fade, _Clock, _ZoomProgress, _Selected, _LoadAvailable, _Pitch, _GridVisibility;
            float _ScanDepth, _FieldGlow, _Vignette, _ScreenInset;
            float mask(sampler2D tex, float2 p, float2 size)
            {
                float2 uv = p / size + .5;
                float inside = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
                return tex2D(tex, uv).r * inside;
            }
            float grid(float2 p)
            {
                // Grid origin equals the upper-left boundary of the same STARWOLF mask.
                float2 cell = (p + _TitleSize.xy * .5) / _Pitch;
                float2 w = fwidth(cell);
                float2 d = abs(frac(cell - .5) - .5) / max(w, .001);
                float fine = 1 - saturate(min(d.x, d.y));
                float2 major = cell / 10;
                float2 mw = fwidth(major);
                float2 md = abs(frac(major - .5) - .5) / max(mw, .001);
                // Fade each tier out as its lines close to within a pixel of each other. Without this
                // the pull-back drives the cells below one pixel and the grid aliases into a flat wash.
                float fineFade = saturate((.5 - max(w.x, w.y)) * 2);
                float majorFade = saturate((.5 - max(mw.x, mw.y)) * 2);
                return fine * .018 * fineFade + (1 - saturate(min(md.x, md.y))) * .04 * majorFade;
            }
            float content(float2 p)
            {
                if (_Stage < .5) return mask(_Ident, p, _IdentSize.xy) * _Fade;
                if (_Stage < 1.5) return mask(_Boot, p * 1.8, _BootSize.xy) * _Fade;
                float ease = _ZoomProgress * _ZoomProgress * (3 - 2 * _ZoomProgress);
                float zoom = lerp(1, 5, ease);
                float2 world = p * zoom;
                float value = grid(world) * _GridVisibility;
                float title = mask(_Title, world, _TitleSize.xy);
                float reveal = _Stage < 2.5 ? saturate((_Phase * 2 - (p.x / _TitleSize.x + .5)) * 8) : 1;
                value += title * reveal * (1 - smoothstep(.55, 1, _ZoomProgress));
                if (_Stage > 3.5)
                {
                    // Reference screen uses upward-positive Y; hit rectangles use downward-positive Y.
                    float n = mask(_NewGame, (p - float2(0,88)) * 1.8, _NewGameSize.xy);
                    float l = mask(_LoadGame, (p - float2(0,-24)) * 1.8, _LoadGameSize.xy);
                    float s = mask(_Settings, (p - float2(0,-136)) * 1.8, _SettingsSize.xy);
                    value += n * (abs(_Selected) < .1 ? 1 : .55);
                    value += l * (_LoadAvailable < .5 ? .18 : abs(_Selected - 1) < .1 ? 1 : .55);
                    value += s * (abs(_Selected - 2) < .1 ? 1 : .55);
                    // A single square identifies keyboard focus; no extra menu labels or ornaments.
                    float2 marker = abs(p - float2(-282, 88 - _Selected * 112));
                    value += step(marker.x, 3) * step(marker.y, 3);
                }
                return value;
            }
            float4 frag(v2f_img i) : SV_Target
            {
                float2 p = (i.uv - .5) * float2(1920,1080);
                float core = content(p);
                float bloom = (content(p + float2(3,0)) + content(p - float2(3,0)) +
                    content(p + float2(0,3)) + content(p - float2(0,3))) * .027;
                bloom += (content(p + float2(7,0)) + content(p - float2(7,0)) +
                    content(p + float2(0,7)) + content(p - float2(0,7))) * .009;
                float grain = frac(sin(dot(floor(i.uv * float2(1920,1080)), float2(12.9898,78.233)) + floor(_Clock*24)) * 43758.5453);
                float flicker = 1 + .002 * sin(_Clock * 17);

                // Terminal presentation, matched to the canonical reference: the tube is never
                // fully dark, scanlines band the whole screen rather than just the glyphs, and the
                // picture falls away into a rounded screen edge. This is all tone — no geometry is
                // warped — so the menu's hit rectangles stay where HandleMenu expects them.
                float scan = 1 - _ScanDepth * .5 * (1 - cos(i.uv.y * 1080 * 3.14159265));
                float2 fromCentre = abs(i.uv - .5) * 2;
                float2 over = max(fromCentre - (1 - _ScreenInset), 0) / max(_ScreenInset, .0001);
                float bezel = saturate(1 - length(over));
                float vignette = 1 - _Vignette * dot(i.uv - .5, i.uv - .5);

                // Grain modulates the lit phosphor rather than being added to the whole field.
                // Added flat it lands on the target's first quantisation step and turns every
                // unlit pixel visibly green, which is why the CRT field read as mid green.
                float3 lit = _Phosphor.rgb * (core + bloom) * (1 + (grain - .5) * .12);
                float3 field = _Background.rgb + _Phosphor.rgb * _FieldGlow;
                return float4((field + lit) * scan * vignette * flicker * bezel, 1);
            }
            ENDCG
        }
    }
}
