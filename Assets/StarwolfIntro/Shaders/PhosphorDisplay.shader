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
        _TitleOffset ("Title offset", Float) = 0
        _Progress ("Loading bar fill", Float) = 0
        _Stage ("Stage", Float) = 0
        _Reveal ("Reveal", Float) = 0
        _Clock ("Clock", Float) = 0
        _Selected ("Selected", Float) = 0
        _LoadAvailable ("Load available", Float) = 0
        // CRT treatment on the lit text. Tune these to taste; nothing in code writes them.
        _ScanDepth ("Scanline depth", Range(0,1)) = 0.18
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
            float _Stage, _Reveal, _Clock, _Selected, _LoadAvailable, _Pitch, _TitleOffset, _Progress;
            float _ScanDepth, _Vignette, _ScreenInset;
            float mask(sampler2D tex, float2 p, float2 size)
            {
                float2 uv = p / size + .5;
                float inside = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
                return tex2D(tex, uv).r * inside;
            }
            // Owner 2026-10-24: each line types in where it sits, left to right, one cell column at a
            // time; the newest column fades up as the next arrives. Then a straight cut. No zoom.
            float typed(float2 q, float2 size)
            {
                float cols = max(1, floor(size.x / _Pitch));
                float col = floor((q.x / size.x + .5) * cols);
                return saturate(_Reveal * cols - col);
            }
            // Owner 2026-10-26: a loading bar under INITIALISING..., in the same dots at the same
            // size: a one-cell outline, a one-cell gap, and a fill that runs left to right.
            float bar(float2 p)
            {
                float s = _Pitch / 1.8, rows = 7;
                float cols = max(5, floor(_BootSize.x / _Pitch));
                float2 q = (p - float2(-cols * s * .5, -90)) / s;
                float2 cell = floor(q);
                float inside = step(0, cell.x) * step(cell.x, cols - 1) * step(0, cell.y) * step(cell.y, rows - 1);
                float lamp = step(frac(q.x), 6.0 / 9) * step(frac(q.y), 6.0 / 9);
                float border = cell.x < .5 || cell.x > cols - 1.5 || cell.y < .5 || cell.y > rows - 1.5;
                float fill = cell.x > 1.5 && cell.y > 1.5 && cell.y < rows - 2.5 && cell.x - 1 <= _Progress * (cols - 4);
                return inside * lamp * saturate(border + fill * (cell.x < cols - 2.5));
            }
            float content(float2 p)
            {
                if (_Stage < .5) return mask(_Ident, p, _IdentSize.xy) * typed(p, _IdentSize.xy);
                if (_Stage < 1.5) return mask(_Boot, p * 1.8, _BootSize.xy) * typed(p * 1.8, _BootSize.xy) + bar(p);
                float2 t = p - float2(0, _TitleOffset);
                if (_Stage < 2.5) return mask(_Title, t, _TitleSize.xy) * typed(t, _TitleSize.xy);
                float value = 0;
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
                    content(p + float2(0,3)) + content(p - float2(0,3))) * .015;
                bloom += (content(p + float2(7,0)) + content(p - float2(7,0)) +
                    content(p + float2(0,7)) + content(p - float2(0,7))) * .009;
                float grain = frac(sin(dot(floor(i.uv * float2(1920,1080)), float2(12.9898,78.233)) + floor(_Clock*24)) * 43758.5453);
                float flicker = 1 + .002 * sin(_Clock * 17);

                // Terminal presentation on the lit text: scanlines, a vignette and a rounded screen
                // edge. Tone only — no geometry is warped — so the menu's hit rectangles stay where
                // HandleMenu expects them. Owner 2026-10-25: the field is a very dark green, as in
                // the reference shots (it was flat black from 2026-10-24).
                float scan = 1 - _ScanDepth * .5 * (1 - cos(i.uv.y * 1080 * 3.14159265));
                float2 fromCentre = abs(i.uv - .5) * 2;
                float2 over = max(fromCentre - (1 - _ScreenInset), 0) / max(_ScreenInset, .0001);
                float bezel = saturate(1 - length(over));
                float vignette = 1 - _Vignette * dot(i.uv - .5, i.uv - .5);

                // Grain modulates the lit phosphor rather than being added to the whole field.
                // Added flat it lands on the target's first quantisation step and turns every
                // unlit pixel visibly green, which is why the CRT field read as mid green.
                float3 lit = _Phosphor.rgb * (core + bloom) * (1 + (grain - .5) * .12) + _Background.rgb;
                return float4(lit * scan * vignette * flicker * bezel, 1);
            }
            ENDCG
        }
    }
}
