using System.Runtime.InteropServices;

namespace Blade.MG.UI.Services
{
    /// <summary>
    /// Minimal text clipboard access for controls like TextBox.
    /// On Windows, talks to the OS clipboard directly via user32.dll/kernel32.dll, which -
    /// unlike System.Windows.Forms.Clipboard - does not require the calling thread to be STA,
    /// so it's safe to call from a game's main loop. On other platforms (or if the Windows
    /// clipboard call fails for any reason), falls back to an in-process clipboard so
    /// cut/copy/paste still work within the app, just without OS-level integration.
    /// </summary>
    public static class ClipboardService
    {
        private static string fallbackClipboard = "";

        public static void SetText(string text)
        {
            text ??= "";

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    SetWindowsClipboardText(text);
                    return;
                }
                catch
                {
                    // Fall through to the in-process fallback below.
                }
            }

            fallbackClipboard = text;
        }

        public static string GetText()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    string clipboardText = GetWindowsClipboardText();
                    if (clipboardText != null)
                    {
                        return clipboardText;
                    }
                }
                catch
                {
                    // Fall through to the in-process fallback below.
                }
            }

            return fallbackClipboard;
        }

        // ---=== Windows clipboard (user32.dll / kernel32.dll) ===---

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private static void SetWindowsClipboardText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return;
            }

            try
            {
                EmptyClipboard();

                int byteCount = (text.Length + 1) * 2; // UTF-16 code units + null terminator
                IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)byteCount);
                if (hGlobal == IntPtr.Zero)
                {
                    return;
                }

                IntPtr target = GlobalLock(hGlobal);
                if (target == IntPtr.Zero)
                {
                    return;
                }

                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                    Marshal.WriteInt16(target, text.Length * 2, 0);
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }

                SetClipboardData(CF_UNICODETEXT, hGlobal);
            }
            finally
            {
                CloseClipboard();
            }
        }

        private static string GetWindowsClipboardText()
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return null;
            }

            try
            {
                IntPtr hGlobal = GetClipboardData(CF_UNICODETEXT);
                if (hGlobal == IntPtr.Zero)
                {
                    return null;
                }

                IntPtr source = GlobalLock(hGlobal);
                if (source == IntPtr.Zero)
                {
                    return null;
                }

                try
                {
                    return Marshal.PtrToStringUni(source);
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }
    }
}
