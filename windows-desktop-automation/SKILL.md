---
name: windows-desktop-automation
description: Automates Windows desktop applications using the ai-hands CLI tool. Captures screenshots, clicks, types text, presses keys, scrolls, manages windows, discovers UI elements via UI Automation, and compares images. Use when the user asks to "test a desktop app", "take a screenshot", "click a button", "type into a window", "interact with a UI", "automate a Windows application", "visual regression test", or "compare screenshots". Works with Win32, .NET, and Electron apps.
compatibility: Requires Windows with .NET 8 runtime. The ai-hands CLI must be built or published first.
metadata:
  author: ai-hands
  version: 1.0.0
  category: desktop-automation
  tags: [windows, ui-testing, automation, screenshots]
---

# Windows Desktop Automation

Interact with Windows desktop applications by invoking the `ai-hands` CLI via Bash. All commands return JSON to stdout.

## Setup

```bash
# During development (from the ai-hands repo root)
dotnet run --project src/AiHands -- <command> [args]

# After publishing (faster startup)
./publish/ai-hands.exe <command> [args]
```

## Instructions

### Step 1: Identify the target window

Before interacting with an app, find it or launch it.

```bash
# List all visible windows
ai-hands window list

# Find a specific window by title (partial match)
ai-hands window find "Notepad"

# Launch an app and wait for its window
notepad.exe &
ai-hands window wait "Notepad" --timeout 5000
```

### Step 2: Focus the window

CRITICAL: Always focus the window before sending input. Otherwise keystrokes and clicks may go to the wrong window.

```bash
ai-hands window focus "Notepad"
```

### Step 3: Interact with the application

Choose the appropriate interaction method:

**Click at coordinates:**
```bash
ai-hands click 500 300                        # Left click (screen coords)
ai-hands click 100 50 --window "Notepad"      # Click relative to window
ai-hands click 500 300 --button right          # Right click
ai-hands click 500 300 --double                # Double click
```

**Type text:**
```bash
ai-hands type "Hello, World!"                 # Type text (30ms default delay)
ai-hands type "slow typing" --delay 50        # Custom inter-key delay
```

**Press key combinations:**
```bash
ai-hands key enter
ai-hands key ctrl+s
ai-hands key ctrl+shift+n
ai-hands key alt+f4
```

Supported modifiers: ctrl, alt, shift, win. Keys: enter, tab, esc, space, backspace, delete, home, end, pageup, pagedown, up/down/left/right, f1-f12, plus single characters.

**Scroll:**
```bash
ai-hands scroll 3                             # Scroll up 3 clicks
ai-hands scroll -3                            # Scroll down
ai-hands scroll 3 --x 500 --y 300             # Scroll at position
ai-hands scroll 3 --horizontal                # Horizontal scroll
```

### Step 4: Use UI Automation for reliable element targeting

Prefer `element find --click` over coordinate-based clicking — it works across resolutions and DPI settings.

```bash
# Discover elements in a window
ai-hands element list "My App" --depth 5
ai-hands element list "My App" --type Button

# Find and click a button by name
ai-hands element find "My App" --name "Submit" --click

# Find a text field and set its value
ai-hands element find "My App" --id "txtEmail" --value "test@example.com"
```

Common element types: Button, Edit, Text, CheckBox, RadioButton, ComboBox, List, ListItem, Menu, MenuItem, Tab, TabItem, Tree, TreeItem, Window, Pane, ToolBar, StatusBar, ScrollBar.

### Step 5: Screenshot to verify

After every interaction, take a screenshot and read it to confirm the result.

```bash
ai-hands screenshot -o shot.png --window "My App"
```

Then use the Read tool to view the PNG and decide the next action.

Additional screenshot options:
```bash
ai-hands screenshot -o full.png                   # Full desktop
ai-hands screenshot -o region.png --region 0,0,800,600  # Screen region
ai-hands screenshot -o delayed.png --delay 1000   # Wait 1s before capture
```

### Step 6: Compare images (optional)

For visual regression testing, compare two screenshots:

```bash
ai-hands diff before.png after.png -o diff.png --threshold 5
```

Returns `diffPercent` and `match` fields. The diff image highlights changed pixels in red.

## Examples

### Example 1: Launch Notepad, type text, close without saving

```bash
notepad.exe &
ai-hands window wait "Notepad" --timeout 5000
ai-hands window focus "Notepad"
ai-hands type "Lorem ipsum dolor sit amet."
ai-hands screenshot -o result.png --window "Notepad"
ai-hands key alt+f4
```

### Example 2: Find and click a button using UI Automation

```bash
ai-hands window focus "My App"
ai-hands element find "My App" --name "Save" --type Button --click
ai-hands screenshot -o after_save.png --window "My App"
```

### Example 3: Visual regression test

```bash
ai-hands screenshot -o baseline.png --window "My App"
# ... perform some action ...
ai-hands screenshot -o current.png --window "My App"
ai-hands diff baseline.png current.png -o diff.png --threshold 10
# Read diff.png to inspect changes
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
