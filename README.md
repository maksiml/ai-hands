# ai-hands

Windows desktop automation CLI for AI agents. Start applications, click, type, scroll, manage windows, discover UI elements, take screenshots, and compare images — all from the command line with JSON output.

Built for [Claude Code](https://claude.ai/code) and other AI agents that need to interact with Windows desktop applications.

## Quick Start

### Option 1: Download the executable (recommended)

No .NET runtime required — download and add to PATH:

```powershell
# PowerShell — download and install
New-Item -ItemType Directory -Force -Path "$env:LOCALAPPDATA\ai-hands"
Invoke-WebRequest -Uri "https://github.com/maksiml/ai-hands/releases/latest/download/ai-hands.exe" -OutFile "$env:LOCALAPPDATA\ai-hands\ai-hands.exe"

# Add to PATH (current user, persistent + current session)
$path = [Environment]::GetEnvironmentVariable("Path", "User")
if ($path -notlike "*ai-hands*") {
    [Environment]::SetEnvironmentVariable("Path", "$path;$env:LOCALAPPDATA\ai-hands", "User")
    $env:Path += ";$env:LOCALAPPDATA\ai-hands"
}
```

Restart your terminal, then verify:

```powershell
ai-hands.exe help
```

> **Note:** In PowerShell, the hyphen in `ai-hands` is interpreted as a minus operator. Always include the `.exe` extension, or use `& ai-hands help`. In bash/cmd this is not an issue.

### Option 2: Clone and build from source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/maksiml/ai-hands.git
cd ai-hands
dotnet publish src/AiHands -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

Then add the `publish` directory to your PATH:

```powershell
# PowerShell — add to PATH (current user, persistent + current session)
$publishDir = (Resolve-Path ./publish).Path
$path = [Environment]::GetEnvironmentVariable("Path", "User")
if ($path -notlike "*$publishDir*") {
    [Environment]::SetEnvironmentVariable("Path", "$path;$publishDir", "User")
    $env:Path += ";$publishDir"
}
```

Restart your terminal, then verify:

```powershell
ai-hands.exe help
```

## Usage

```
ai-hands <command> [options]

Commands:
  screenshot   Capture screen, window, or region to PNG
  click        Mouse click at coordinates
  type         Type text string
  key          Press key combination
  scroll       Mouse wheel scroll
  window       Window management (find, focus, move, resize, etc.)
  element      UI Automation element discovery and interaction
  diff         Compare two images
```

All commands return JSON to stdout:

```json
{"ok": true, "file": "screenshot.png", "width": 1920, "height": 1080}
```

### Examples

```bash
# List all visible windows
ai-hands window list

# Launch an app and wait for it
notepad.exe &
ai-hands window wait "Notepad" --timeout 5000

# Focus, type, and screenshot
ai-hands window focus "Notepad"
ai-hands type "Hello from ai-hands!"
ai-hands screenshot --output screenshot.png --window "Notepad"

# Click at window-relative coordinates
ai-hands click 100 50 --window "Notepad"

# Press key combinations
ai-hands key ctrl+s

# Scroll down
ai-hands scroll -3 --x 500 --y 300

# Find and click a UI element by name
ai-hands element find "My App" --name "Save" --click

# Compare two screenshots
ai-hands diff before.png after.png --output diff.png --threshold 10
```

## Installing as a Claude Code Skill

ai-hands includes a [skill definition](windows-desktop-automation/SKILL.md) that teaches Claude Code how to use it. To install:

**Project-level** (for a specific project):

```bash
mkdir -p .claude/skills
cp -r windows-desktop-automation .claude/skills/
```

**Personal** (available in all your projects):

```bash
mkdir -p ~/.claude/skills
cp -r windows-desktop-automation ~/.claude/skills/
```

Once installed, Claude Code will automatically use ai-hands when you ask it to interact with desktop applications.

## Requirements

- Windows 10 or later
- No runtime dependencies when using the pre-built executable
- .NET 8 SDK only required for building from source

## License

MIT
