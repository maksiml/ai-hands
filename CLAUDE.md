# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

`ai-hands` is a Windows desktop automation CLI tool. It lets AI agents (including Claude Code) start applications, interact with their UI, take screenshots, and compare visual results. Built in C# / .NET 8.

## Build & Run

```bash
# Build
dotnet build src/AiHands/AiHands.csproj

# Run (dev)
dotnet run --project src/AiHands -- <command> [args]

# Publish self-contained single-file executable
pwsh publish.ps1
# Or manually:
dotnet publish src/AiHands -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

## Architecture

Single .NET 8 console app. No test project yet.

```
src/AiHands/
├── Program.cs              # Entry point, command dispatch
├── Commands/               # One file per CLI command (ScreenshotCommand, ClickCommand, etc.)
├── Automation/             # Core logic (ScreenCapture, InputSimulator, WindowManager, UiaHelper, ImageDiff)
└── Infrastructure/         # CliParser, JsonOutput helpers
```

**Key design decisions:**
- Hand-rolled CLI parser (no System.CommandLine) for fast startup — this tool is called repeatedly by agents
- `net8.0-windows` TFM with `UseWindowsForms=true` solely for GDI+ System.Drawing support
- P/Invoke via manual DllImport (CsWin32 package is referenced but not yet used for all calls)
- UI Automation via `System.Windows.Automation` framework assemblies
- All output is JSON to stdout: `{"ok":true,...}` on success, `{"ok":false,"error":"..."}` on failure
- Exit codes: 0=success, 1=error, 2=bad args, 3=timeout, 4=not found

**Adding a new command:**
1. Create `Commands/NewCommand.cs` with a static `Run(string[] args)` method returning `int`
2. Wire it into the switch in `Program.cs`
3. Use `CliParser` for arg parsing and `JsonOutput.Success/Error` for output

## Code Style

- All public classes, records, enums, and methods must have XML documentation comments (`/// <summary>`)
- Keep doc comments concise — describe *what* it does, not *how*
- Add `<param>` tags only when the parameter meaning isn't obvious from the name
- Private methods do not need doc comments

## Git Workflow

- Never commit or push without explicit user request
- Always show `git status` and `git diff` before committing so the user can review
