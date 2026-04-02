using System.Runtime.InteropServices;

namespace AiHands.Automation;

/// <summary>
/// Specifies which mouse button to use for a click action.
/// </summary>
public enum MouseButton { Left, Right, Middle }

/// <summary>
/// Simulates mouse clicks, keyboard input, key presses, and scroll events via Win32 SendInput.
/// </summary>
public static class InputSimulator
{
    #region P/Invoke

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public INPUTUNION U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx, dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;

    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint MOUSEEVENTF_HWHEEL = 0x1000;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    private const uint MAPVK_VK_TO_VSC = 0;

    #endregion

    /// <summary>
    /// Performs a mouse click at the specified screen coordinates.
    /// </summary>
    public static void Click(int x, int y, MouseButton button = MouseButton.Left, bool doubleClick = false)
    {
        SetCursorPos(x, y);
        Thread.Sleep(10); // Small delay for cursor to settle

        var (downFlag, upFlag) = button switch
        {
            MouseButton.Left => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
            MouseButton.Right => (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            MouseButton.Middle => (MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP),
            _ => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP)
        };

        SendMouseEvent(downFlag);
        SendMouseEvent(upFlag);

        if (doubleClick)
        {
            Thread.Sleep(30);
            SendMouseEvent(downFlag);
            SendMouseEvent(upFlag);
        }
    }

    /// <summary>
    /// Types a string of text using Unicode key events.
    /// </summary>
    public static void TypeText(string text, int delayMs = 30)
    {
        foreach (char c in text)
        {
            // Send key-down
            var down = new INPUT
            {
                Type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)c,
                        dwFlags = KEYEVENTF_UNICODE
                    }
                }
            };
            SendInput(1, [down], Marshal.SizeOf<INPUT>());

            // Brief pause to let the app process the key-down before key-up
            Thread.Sleep(5);

            // Send key-up
            var up = new INPUT
            {
                Type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)c,
                        dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP
                    }
                }
            };
            SendInput(1, [up], Marshal.SizeOf<INPUT>());

            if (delayMs > 0)
                Thread.Sleep(delayMs);
        }
    }

    /// <summary>
    /// Sends a key combination such as "ctrl+s" or "alt+f4".
    /// </summary>
    public static void KeyPress(string keyCombo)
    {
        var keys = keyCombo.ToLowerInvariant().Split('+').Select(k => k.Trim()).ToArray();
        var vkCodes = keys.Select(MapKeyName).ToArray();

        // Press all keys down
        foreach (var vk in vkCodes)
            SendKeyEvent(vk, false);

        // Release in reverse order
        foreach (var vk in vkCodes.Reverse())
            SendKeyEvent(vk, true);
    }

    /// <summary>
    /// Sends a mouse scroll event, optionally at a specific screen position.
    /// </summary>
    public static void Scroll(int amount, int x = -1, int y = -1, bool horizontal = false)
    {
        if (x >= 0 && y >= 0)
            SetCursorPos(x, y);

        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new INPUTUNION
            {
                mi = new MOUSEINPUT
                {
                    mouseData = (uint)(amount * 120), // WHEEL_DELTA = 120
                    dwFlags = horizontal ? MOUSEEVENTF_HWHEEL : MOUSEEVENTF_WHEEL
                }
            }
        };

        SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    private static void SendMouseEvent(uint flags)
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new INPUTUNION
            {
                mi = new MOUSEINPUT { dwFlags = flags }
            }
        };
        SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    private static void SendKeyEvent(ushort vk, bool keyUp)
    {
        var scan = (ushort)MapVirtualKey(vk, MAPVK_VK_TO_VSC);
        var input = new INPUT
        {
            Type = INPUT_KEYBOARD,
            U = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scan,
                    dwFlags = keyUp ? KEYEVENTF_KEYUP : 0
                }
            }
        };
        SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    private static ushort MapKeyName(string name) => name switch
    {
        "ctrl" or "control" => 0x11,  // VK_CONTROL
        "alt" or "menu" => 0x12,       // VK_MENU
        "shift" => 0x10,               // VK_SHIFT
        "win" or "windows" or "lwin" => 0x5B, // VK_LWIN
        "enter" or "return" => 0x0D,   // VK_RETURN
        "tab" => 0x09,                 // VK_TAB
        "esc" or "escape" => 0x1B,     // VK_ESCAPE
        "space" => 0x20,               // VK_SPACE
        "backspace" or "back" => 0x08, // VK_BACK
        "delete" or "del" => 0x2E,     // VK_DELETE
        "insert" or "ins" => 0x2D,     // VK_INSERT
        "home" => 0x24,                // VK_HOME
        "end" => 0x23,                 // VK_END
        "pageup" or "pgup" => 0x21,    // VK_PRIOR
        "pagedown" or "pgdn" => 0x22,  // VK_NEXT
        "up" => 0x26,                  // VK_UP
        "down" => 0x28,                // VK_DOWN
        "left" => 0x25,                // VK_LEFT
        "right" => 0x27,               // VK_RIGHT
        "f1" => 0x70, "f2" => 0x71, "f3" => 0x72, "f4" => 0x73,
        "f5" => 0x74, "f6" => 0x75, "f7" => 0x76, "f8" => 0x77,
        "f9" => 0x78, "f10" => 0x79, "f11" => 0x7A, "f12" => 0x7B,
        "printscreen" or "prtsc" => 0x2C, // VK_SNAPSHOT
        "scrolllock" => 0x91,          // VK_SCROLL
        "pause" => 0x13,               // VK_PAUSE
        "numlock" => 0x90,             // VK_NUMLOCK
        "capslock" => 0x14,            // VK_CAPITAL
        _ when name.Length == 1 => (ushort)(VkKeyScan(name[0]) & 0xFF),
        _ => throw new ArgumentException($"Unknown key: {name}")
    };
}
