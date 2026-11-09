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
        _Flicker ("Full intro: words flicker in", Float) = 0
        _Change ("Seconds into the flicker", Float) = 0
        _ChangeSeconds ("Flicker length", Float) = 0.5
        _Left ("Shared left edge", Float) = 0
        _Diffusion ("Diffusion strength", Float) = 0.7
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
            float _Flicker, _Change, _ChangeSeconds, _Left, _Diffusion;
            float hash3(float3 v) { return frac(sin(dot(v, float3(12.9898, 78.233, 37.719))) * 43758.5453); }
            // Owner 2026-11-09: in the full intro a word flickers into being where it stands, square by
            // square in a quick scattered order, and SECTION 36 turns into STARWOLF the same way. Both
            // words share one box (sized to each other), so the squares lit in both (the S, the frame) stay lit.
            // 0 = the old picture, 1 = the new, for this pixel's square.
            float settled(float2 p)
            {
                float2 c = floor((p - float2(_Left, _TitleOffset - _IdentSize.y * .5)) / _Pitch);
                float w = max(.01, _ChangeSeconds), at = hash3(float3(c, 1)) * w * .75;
                float coin = step(.5, hash3(float3(c, floor(_Clock * 30)))); // flickers at 30 a second
                return _Change >= at + w * .25 ? 1 : _Change >= at ? coin : 0;
            }
            float mask(sampler2D tex, float2 p, float2 size)
            {
                float2 uv = p / size + .5;
                float inside = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
                return tex2D(tex, uv).r * inside;
            }
            // Owner 2026-10-24: INITIALISING... and the title-only pages type in where they stand, left to
            // right, one cell column at a time; the newest column fades up as the next arrives. No zoom.
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
                float2 t = p - float2(0, _TitleOffset);
                float value = 0;
                if (_Stage < .5) value = mask(_Ident, t, _IdentSize.xy) * settled(p);
                else if (_Stage < 1.5) value = mask(_Boot, p * 1.8, _BootSize.xy) * typed(p * 1.8, _BootSize.xy) + bar(p);
                else if (_Stage < 2.5 && _Flicker > .5) value = lerp(mask(_Ident, t, _IdentSize.xy), mask(_Title, t, _TitleSize.xy), settled(p));
                else if (_Stage < 2.5) value = mask(_Title, t, _TitleSize.xy) * typed(t, _TitleSize.xy);
                else
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
            // 1 inside the words' box (and a little round it), 0 elsewhere on the screen.
            float near(float2 p)
            {
                float2 d = abs(p - float2(_Left + _IdentSize.x * .5, _TitleOffset)) - _IdentSize.xy * .5;
                return step(max(d.x, d.y), 24);
            }
            float4 frag(v2f_img i) : SV_Target
            {
                float2 p = (i.uv - .5) * float2(1920,1080);
                // Owner 2026-11-09: while a word flickers in or over, a little horizontal diffusion and
                // grain: thin bands of the picture slip sideways and the light smears along them. It
                // dies away as the squares settle. Never on the menu, so its hit rectangles stay put.
                float active = (_Stage < .5 || (_Stage > 1.5 && _Stage < 2.5 && _Flicker > .5))
                    ? saturate(1 - _Change / max(.01, _ChangeSeconds * 1.3)) * _Diffusion : 0;
                float frame = floor(_Clock * 30), band = floor(i.uv.y * 1080 / 3);
                float slip = step(.72, hash3(float3(band, frame, 7))) * (hash3(float3(band, frame, 3)) - .5) * 28 * active;
                p.x += slip;
                float core = content(p);
                float bloom = (content(p + float2(3,0)) + content(p - float2(3,0)) +
                    content(p + float2(0,3)) + content(p - float2(0,3))) * .015;
                bloom += (content(p + float2(7,0)) + content(p - float2(7,0)) +
                    content(p + float2(0,7)) + content(p - float2(0,7))) * .009;
                if (active > 0)
                {
                    float smear = 0;
                    for (int s = 1; s <= 4; s++) smear += (content(p + float2(s * 5, 0)) + content(p - float2(s * 5, 0))) * (.16 / s);
                    bloom += smear * active * (.4 + abs(slip) / 14);
                }
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
                // While a word changes, the grain is stronger, and the slipped bands get a faint glow of their own.
                float3 lit = _Phosphor.rgb * (core + bloom) * (1 + (grain - .5) * (.12 + .7 * active)) + _Background.rgb
                           + _Phosphor.rgb * grain * .05 * active * step(.01, abs(slip)) * near(p);
                return float4(lit * scan * vignette * flicker * bezel, 1);
            }
            ENDCG
        }
    }
}
