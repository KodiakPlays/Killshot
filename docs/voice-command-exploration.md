# Voice Command Exploration

**Summary.** The captain could speak orders ("come about to port", "all ahead full", "sweep bearing
045") and the crew would carry them out. This is an idea, not built. It comes after the board is
finished. It is not hard: every button already goes through one function,
`BoardManager.Click(action, arg)`, so a spoken order only has to reach that same function.
The hard part is turning speech into words, and Whisper already does that.

Explored with the owner on 2026-10-26. The decision itself is BUILD_LOG decision 36.

## What the owner decided

- **Voice is an extra way to give orders.** It never replaces the buttons.
- **The crew takes a moment** to carry an order out. Buttons stay instant.
- **The crew answers softly** — a quiet "roger", not chatter.
- **Offline only.** Speech is heard on the player's own machine. No online AI.
- Push-to-talk. The board shows what it heard. "Say again" when it misses. Firing stays on buttons.

## What it sounds like

| The captain says | What the board does |
|---|---|
| "Come about to port" | steps the ordered heading |
| "All ahead full" | sets the throttle |
| "Scanner, go active" | `sensors.active` |
| "Sweep bearing 045" | sets the scan cone's bearing, then goes active |
| "Sweep there" (while pointing at the plot) | the same, at the point clicked |

## How it works

### Everything goes through one door that already exists

When you click a button, the board calls `Click` with the button's name, for example
`Click("sensors.active")`. The board already knows what each name does, refuses orders the ship
can't carry out, and plays the sound. So any new way of giving orders — voice, Claude, a phone —
only has to produce those same names. The rules come for free.

**Keep it that way.** Until this is built, every action must keep going through `Click`. The board
refactor must not break that.

### Talking to a built game from outside: the door

A built game (.exe) is a closed box. To control it from another program, the game opens a door.

Programs on one computer send each other text through numbered slots called **ports**. Think of the
computer as an apartment building and the port as an apartment number. The game says "I'm listening
at 7777". Any program on the same computer can then connect to 7777 and send it lines of text.

One order, start to finish:

1. The game starts and listens at 7777, on this computer only.
2. Something connects — a terminal window, Claude, a voice app.
3. It sends the line `sensors.active`.
4. The game reads the line and calls `Click("sensors.active")`.
5. The sensors go active, the lamp lights, the sound plays — exactly as if clicked.

You can try it by hand with no Claude: open Terminal, type `nc localhost 7777`, then type orders.

### The one wrinkle: the tray

Waiting for messages would freeze the game, so a helper waits in the background. Unity does not let
that helper touch the game directly. So the helper drops each order in a **tray**, and the game
empties the tray every frame (about 60 times a second) and runs each order.

The tray is also where the crew's "moment" and the quiet "roger" belong, so every route gets them.

```
buttons ─────────────────┐
voice inside the game ───┼──→ tray → Click → panel moves
door ← MCP ← Claude ─────┘
```

### Roughly the whole door

A sketch to show the size. Not in the project.

```csharp
public class OrderDoor : MonoBehaviour
{
    [SerializeField] BoardManager board;
    readonly ConcurrentQueue<string> tray = new ConcurrentQueue<string>();

    void Start() => new Thread(Listen) { IsBackground = true }.Start(); // the helper

    void Listen() // runs in the background, forever
    {
        var door = new TcpListener(IPAddress.Loopback, 7777); // "listening at 7777", this computer only
        door.Start();
        while (true)
        {
            var reader = new StreamReader(door.AcceptTcpClient().GetStream()); // someone connected
            string line;
            while ((line = reader.ReadLine()) != null) tray.Enqueue(line); // each line → a note in the tray
        }
    }

    void Update() // the game, every frame
    {
        while (tray.TryDequeue(out var line)) // take each note out
        {
            var parts = line.Split(' '); // "helm.heading 045" → "helm.heading", "045"
            board.Click(parts[0], parts.Length > 1 ? parts[1] : null); // same as pressing the button
        }
    }
}
```

A real one would also answer each line ("ok", "refused: no power", "unknown order"), answer a
`status` question (heading, speed, contacts, damage), and stay closed unless a setting opens it.

### Gaps to fill

Most orders already exist as clicks. Heading, speed and the scan cone's bearing are drags today, so
they need simple named orders such as `helm.heading 045`. Each would call a function that already
exists: `nav.SetThrottle`, `nav.StepOrderedHeading`, `SetCone`.

Some keys skip `Click` today. `P` and `1` to `6` go through it. `W`, `S`, `A` and `D` change the
throttle and heading directly (`HelmPanel`). `C` and the arrow keys move the plot directly
(`PlotPanel`).

## The two ways to use it

**1. Voice inside the game — what players get.** The speech model (Whisper, about 75–150 MB) ships
inside the game. Words become a line like `sensors.active`, and the line goes straight into the tray.
No door needed. Offline.

**2. Claude through MCP — for us.** MCP is how Claude plugs into tools. A small MCP program connects
to the door, gives Claude a menu of orders, and sends whichever one Claude picks. This is exactly how
Claude drives the Unity editor today, through the Unity MCP plugin. That plugin does not go into a
built game, so the game needs its own door. It needs internet and a Claude account, so it suits
playtests and demos, not players.

## How hard each piece is

| Piece | Difficulty |
|---|---|
| The door and tray | Easy — about an afternoon |
| The missing named orders (heading, speed, cone) | Easy |
| The MCP program for Claude | Easy |
| Voice inside the game | Medium. "Full speed" is easy; "zero four five" needs tuning to come through reliably. Adds about half a second to a second of delay. |

## Still open

1. **How long is the crew's moment?** Start around 1–2 seconds and adjust in play.
2. **Game time or real time?** The rule is that everything runs in game time, which would make the
   crew nearly instant at x10. Real time may suit people talking better. Owner's call.
3. **The crew's voice.** A few short recorded clips are enough as placeholders.
4. **The door is a new script.** Ask the owner before adding it.
