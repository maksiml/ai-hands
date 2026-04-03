---
name: windows-desktop-automation
description: Automates Windows desktop applications using the ai-hands CLI tool. Captures screenshots, clicks, types text, presses keys, scrolls, manages windows, discovers UI elements via UI Automation, and compares images. Use when the user asks to "test a desktop app", "take a screenshot", "click a button", "type into a window", "interact with a UI", "automate a Windows application", "visual regression test", or "compare screenshots". Works with Win32, .NET, and Electron apps.
compatibility: Requires Windows with .NET 8 runtime. The ai-hands CLI must be built or published first.
metadata:
  author: ai-hands
  version: 1.1.0
  category: desktop-automation
  tags: [windows, ui-testing, automation, screenshots]
---

# Windows Desktop Automation

Interact with Windows desktop applications by invoking the `ai-hands` CLI via Bash. All commands return JSON to stdout.

## Setup

The `ai-hands` executable must be on the system PATH. See the [README](https://github.com/maksiml/ai-hands#quick-start) for installation instructions.

```bash
ai-hands <command> [args]
```

## Command Reference

### screenshot — Capture screen, window, or region to PNG

```bash
ai-hands screenshot --output <path> [--window <title>] [--handle <hex>] [--region x,y,w,h] [--delay <ms>]
```

| Argument | Alias | Required | Default | Description |
|----------|-------|----------|---------|-------------|
| `--output` | `-o` | YES | — | Output PNG file path |
| `--window` | — | no | — | Capture specific window by title (substring, case-insensitive) |
| `--handle` | — | no | — | Capture window by hex handle (e.g. `0x1A2B3C`) |
| `--region` | — | no | — | Capture screen region as `x,y,width,height` (no spaces) |
| `--delay` | — | no | 0 | Wait N milliseconds before capture |

**Capture priority:** `--region` > `--window` > `--handle` > full screen.

**Output:** `{"ok":true, "file":"...", "width":N, "height":N}`

```bash
ai-hands screenshot --output full.png                        # Full desktop
ai-hands screenshot -o win.png --window "Notepad"            # Single window
ai-hands screenshot -o handle.png --handle 0x1A2B3C          # Window by handle
ai-hands screenshot -o region.png --region 0,0,800,600       # Screen region
ai-hands screenshot -o delayed.png --delay 2000              # Delay 2s first
```

---

### click — Mouse click at coordinates

```bash
ai-hands click <x> <y> [--button left|right|middle] [--double] [--window <title>]
```

| Argument | Required | Default | Description |
|----------|----------|---------|-------------|
| `<x>` | YES | — | X coordinate (integer) |
| `<y>` | YES | — | Y coordinate (integer) |
| `--button` | no | `left` | Mouse button: `left`, `right`, `middle` |
| `--double` | no | false | Double-click (flag, no value) |
| `--window` | no | — | Window title — makes x,y relative to window top-left |

**Coordinate system:**
- Without `--window`: x,y are absolute screen coordinates
- With `--window`: x,y are relative to the window's top-left corner; the tool auto-converts to screen coords

**Output:** `{"ok":true, "x":N, "y":N, "button":"left", "double":false}`

```bash
ai-hands click 500 300                          # Absolute screen click
ai-hands click 100 50 --window "Notepad"        # Window-relative click
ai-hands click 500 300 --button right            # Right click
ai-hands click 500 300 --double                  # Double click
```

---

### type — Type text string

```bash
ai-hands type <text> [--delay <ms>]
```

| Argument | Required | Default | Description |
|----------|----------|---------|-------------|
| `<text>` | YES | — | Text to type (Unicode supported) |
| `--delay` | no | `10` | Milliseconds between characters |

**Output:** `{"ok":true, "length":N, "text":"..."}`

**IMPORTANT — no newline support:** The type command sends each character as a Unicode key event. It does NOT interpret `\n` as Enter. To type multi-line text, use `type` for each line and `key enter` between them:

```bash
ai-hands type "First line"
ai-hands key enter
ai-hands type "Second line"
```

**Tip — quoting:** Shell-quote text with double quotes. For text containing special characters, be aware of shell escaping rules.

```bash
ai-hands type "Hello, World!"
ai-hands type "fast input" --delay 5
ai-hands type "slow input" --delay 50
```

---

### key — Press key combination

```bash
ai-hands key <keys>
```

Keys are `+` separated, case-insensitive: `ctrl+s`, `alt+f4`, `shift+ctrl+n`, `enter`.

**Supported key names:**

| Category | Keys |
|----------|------|
| Modifiers | `ctrl` (or `control`), `alt` (or `menu`), `shift`, `win` (or `windows`, `lwin`) |
| Navigation | `up`, `down`, `left`, `right`, `home`, `end`, `pageup` (or `pgup`), `pagedown` (or `pgdn`) |
| Editing | `backspace` (or `back`), `delete` (or `del`), `insert` (or `ins`) |
| Whitespace | `enter` (or `return`), `tab`, `space` |
| Escape | `esc` (or `escape`) |
| Function | `f1` through `f12` |
| Locks | `capslock`, `numlock`, `scrolllock` |
| System | `printscreen` (or `prtsc`), `pause` |
| Any single char | `a`, `b`, `1`, etc. (mapped via VkKeyScan) |

**Output:** `{"ok":true, "keys":"ctrl+s"}`

```bash
ai-hands key enter
ai-hands key ctrl+s
ai-hands key ctrl+shift+n
ai-hands key alt+f4
ai-hands key ctrl+home          # Jump to start of document
ai-hands key ctrl+end           # Jump to end of document
ai-hands key f5
```

---

### scroll — Mouse wheel scroll

```bash
ai-hands scroll <amount> [--x <px>] [--y <px>] [--horizontal]
```

| Argument | Required | Default | Description |
|----------|----------|---------|-------------|
| `<amount>` | YES | — | Scroll clicks: **positive = up, negative = down** |
| `--x` | no | current cursor | X position to scroll at |
| `--y` | no | current cursor | Y position to scroll at |
| `--horizontal` | no | false | Horizontal scroll (flag) |

**Output:** `{"ok":true, "amount":N, "direction":"vertical"}`

```bash
ai-hands scroll 5                       # Scroll up 5 clicks
ai-hands scroll -5                      # Scroll down 5 clicks
ai-hands scroll -3 --x 960 --y 500     # Scroll down at specific position
ai-hands scroll 2 --horizontal          # Horizontal scroll right
```

---

### window — Window management

```bash
ai-hands window <action> [options]
```

All window actions support `--regex` for regex title matching (default is substring, case-insensitive). Actions that target a single window accept either a positional `<title>` or `--handle <hex>`.

#### window list
```bash
ai-hands window list
```
Returns all visible windows with handle, title, className, x, y, width, height.

#### window find
```bash
ai-hands window find <title> [--regex]
```
Returns matching windows array.

#### window focus
```bash
ai-hands window focus <title> [--regex]
ai-hands window focus --handle <hex>
```
**CRITICAL:** Always focus a window before sending keyboard/mouse input. Otherwise input goes to whatever window currently has focus.

#### window wait
```bash
ai-hands window wait <title> [--timeout <ms>] [--regex]
```
Polls every 250ms until window appears. Default timeout: `10000` ms. Returns exit code 3 on timeout.

#### window move / resize
```bash
ai-hands window move <title> <x> <y> [--regex] [--handle <hex>]
ai-hands window resize <title> <width> <height> [--regex] [--handle <hex>]
```

#### window minimize / maximize / restore
```bash
ai-hands window minimize <title> [--regex] [--handle <hex>]
ai-hands window maximize <title> [--regex] [--handle <hex>]
ai-hands window restore <title> [--regex] [--handle <hex>]
```

---

### element — UI Automation element discovery and interaction

Prefer `element find --click` over coordinate-based clicking when possible — it works across resolutions and DPI settings.

#### element list
```bash
ai-hands element list <window-title> [--name <filter>] [--type <filter>] [--id <filter>] [--depth <n>]
```

| Argument | Required | Default | Description |
|----------|----------|---------|-------------|
| `<window-title>` | YES | — | Window to enumerate |
| `--name` | no | — | Filter by name (substring, case-insensitive) |
| `--type` | no | — | Filter by control type (exact, case-insensitive) |
| `--id` | no | — | Filter by automation ID (substring, case-insensitive) |
| `--depth` | no | `3` | Max tree depth to traverse |

Common types: Button, Edit, Text, CheckBox, RadioButton, ComboBox, List, ListItem, Menu, MenuItem, Tab, TabItem, Tree, TreeItem, Window, Pane, ToolBar, StatusBar, ScrollBar.

#### element find
```bash
ai-hands element find <window-title> [--name <text>] [--type <type>] [--id <id>] [--click] [--value <text>]
```
At least one of `--name`, `--type`, or `--id` is required. Returns exit code 4 if not found.

- `--click` clicks the element's center
- `--value` sets the element's text (requires ValuePattern support)

```bash
ai-hands element list "My App" --depth 5
ai-hands element find "My App" --name "Submit" --click
ai-hands element find "My App" --id "txtEmail" --value "test@example.com"
```

---

### diff — Compare two images

```bash
ai-hands diff <image1> <image2> [--output <path>] [--threshold <0-255>]
```

| Argument | Alias | Required | Default | Description |
|----------|-------|----------|---------|-------------|
| `<image1>` | — | YES | — | First image path |
| `<image2>` | — | YES | — | Second image path |
| `--output` | `-o` | no | — | Save diff visualization PNG (red = different, dimmed = same) |
| `--threshold` | — | no | `10` | Per-channel tolerance (0-255) |

**Output:** `{"ok":true, "match":bool, "diffPercent":N, "diffPixels":N, "totalPixels":N, "diffImage":"..."}`

```bash
ai-hands diff before.png after.png
ai-hands diff before.png after.png -o diff.png --threshold 5
```

---

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error |
| 2 | Bad arguments / usage error |
| 3 | Timeout (e.g. window wait) |
| 4 | Not found (e.g. element find) |

---

## Instructions

### Step 1: Identify the target window

Before interacting with an app, find it or launch it.

```bash
ai-hands window list
ai-hands window find "Notepad"

# Launch and wait
notepad.exe &
ai-hands window wait "Notepad" --timeout 5000
```

### Step 2: Focus the window

**CRITICAL:** Always focus the window before sending input. Otherwise keystrokes and clicks may go to the wrong window.

```bash
ai-hands window focus "Notepad"
```

### Step 3: Interact with the application

Use click, type, key, scroll, or element commands as needed.

### Step 4: Screenshot to verify

After every significant interaction, take a screenshot and read it to confirm the result.

```bash
ai-hands screenshot -o shot.png --window "My App"
```

Then use the Read tool to view the PNG and decide the next action.

---

## Practical Tips and Gotchas

### Window title matching can be ambiguous

`window find "Obsidian" --regex` matches ANY window whose title contains "Obsidian" — including your own terminal if the task description contains that word. When multiple windows match:

1. Use `window find` or `window list` first to see all matches
2. Identify the correct window by its `className` or full title
3. Use `--handle <hex>` to target the exact window for focus, screenshot, etc.

```bash
# BAD: might match the wrong window
ai-hands window focus "Obsidian" --regex

# GOOD: find first, then target by handle
ai-hands window find "Obsidian" --regex
# Pick the right handle from results
ai-hands window focus --handle 0x1C09AE
ai-hands screenshot --handle 0x1C09AE --output shot.png
```

### Multi-line text requires key enter between lines

The `type` command does NOT interpret newlines. Type each line separately:

```bash
ai-hands type "Line one"
ai-hands key enter
ai-hands type "Line two"
```

### Apps may interpret or transform typed text

The `type` command sends raw keystrokes. The receiving application may interpret them according to its own logic — auto-completing, auto-formatting, escaping special characters, triggering shortcuts, or restructuring content. This can happen in any app: markdown editors, Word, Excel, browsers, chat apps, IDEs, etc.

Adjust `--delay` to give the app more time to settle between keystrokes

### Always chain type and key commands sequentially

Don't run `type` and `key` in parallel — they share the same keyboard input channel and will interleave unpredictably.

### Use sleep between launch and interaction

After launching an app, use `window wait` or `sleep` before interacting. Apps need time to fully render:

```bash
"C:\path\to\app.exe" &
sleep 5
ai-hands window wait "App Title" --timeout 15000
ai-hands window focus "App Title"
```

### Use the system temp directory for screenshots

Save screenshots to the Windows temp directory, which always exists. Use the full path to avoid shell-dependent variable syntax:

```
C:\Users\<username>\AppData\Local\Temp\shot.png
```

**Note:** The `--output` path's parent directory must exist — the tool will not create it.

### Coordinate tips 

- Use `screenshot` + Read to visually identify click targets
- Use `element find --click` when possible (resolution-independent)
- With `--window` on click, coordinates are relative to window top-left
- Without `--window`, coordinates are absolute screen position

---

## Examples

### Example 1: Launch Notepad, type text, save and close

```bash
notepad.exe &
ai-hands window wait "Notepad" --timeout 5000
ai-hands window focus "Notepad"
ai-hands type "Hello from ai-hands!"
ai-hands screenshot --output result.png --window "Notepad"
ai-hands key ctrl+s
# Handle Save dialog...
ai-hands key alt+f4
```

### Example 2: Interact with app using UI Automation

```bash
ai-hands window focus "My App"
ai-hands element list "My App" --depth 5 --type Button
ai-hands element find "My App" --name "Save" --type Button --click
ai-hands screenshot --output after_save.png --window "My App"
```

### Example 3: Visual regression test

```bash
ai-hands screenshot --output baseline.png --window "My App"
# ... perform actions ...
ai-hands screenshot --output current.png --window "My App"
ai-hands diff baseline.png current.png --output diff.png --threshold 10
```

### Example 4: Send a message in a chat app (e.g. Teams)

```bash
# Launch and wait
"C:\path\to\app.exe" &
ai-hands window wait "Teams" --timeout 15000

# Find the exact window (avoid title ambiguity)
ai-hands window list
ai-hands window focus --handle 0xABCDEF

# Navigate with keyboard shortcuts
ai-hands key ctrl+n                    # New chat
ai-hands type "user@example.com"       # Type recipient
sleep 2                                # Wait for search results
ai-hands key enter                     # Select suggestion

# Click message input and send
ai-hands click 700 770 --window "Teams"
ai-hands type "Hello!"
ai-hands key enter                     # Send
```

## Troubleshooting

### Error: Window not found
Cause: Title doesn't match any visible window. Titles use partial matching.
Solution: Run `ai-hands window list` to see all window titles, then adjust your query.

### Error: Timeout waiting for window
Cause: App hasn't launched yet or title changed.
Solution: Increase `--timeout` or verify the app is running with `ai-hands window list`.

### Error: Typed text is garbled or has duplicate characters
Cause: Inter-key delay too low for the target app.
Solution: Increase delay with `--delay 50` or higher.

### Error: Click lands on wrong position
Cause: DPI scaling mismatch or window moved between commands.
Solution: Use `--window` flag for window-relative coordinates, or use `element find --click` instead.

### Error: Element not found
Cause: Element not visible at the searched tree depth, or name/type doesn't match.
Solution: Increase `--depth`, or use `element list` first to discover available elements and their exact names.

### Error: Output directory does not exist
Cause: The parent directory for `--output` doesn't exist.
Solution: Use the system temp directory (`$LOCALAPPDATA/Temp/`) which always exists.

### Typed text is mangled, reformatted, or triggers unintended behavior
Cause: The application interpreted keystrokes according to its own logic (auto-complete, auto-format, shortcut triggers, character escaping, etc.).
Solution: Type in smaller chunks and verify with screenshots. Disable auto-format features, dismiss popups with `key esc`, or increase `--delay`.
