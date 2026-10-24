# QR Sniper

> **Version:** `v0.9.8`  
> Ultra-low memory footprint (~3–5 MB RAM), instant 100% standalone on-screen QR code scanner for clean Windows 10 & 11 installations.

---

## Performance & Footprint

| Metric | Value |
|---|---|
| **Idle Memory (RAM)** | **~3–5 MB** (native Working Set trim — practically invisible in Task Manager) |
| **Idle CPU** | **0.0%** (zero background load, native Win32 `RegisterHotKey`) |
| **Decode Latency** | **~10–15 ms** (instant C++20 engine with subpixel sampling) |
| **Binary Size** | **3.12 MB** (100% standalone single `.exe`, zero external dependencies) |
| **Clean OS Support** | **Runs out-of-the-box on clean Windows 10/11 without extra runtimes** |

---

## Features

- **Instant Full-Screen Scan:**
  Press your global shortcut (default: `Ctrl + Shift + Q`) to capture and decode all QR codes on screen across multi-monitor setups and high-DPI displays.
- **Modern High-Speed QR Engine (ZXing-C++ 2.x):**
  - **Unmatched Decoding Quality:** Easily reads stylized, fluid-dot, low-contrast dark mode, inverted, distorted, rotated, heavy branded (center logo), and AI-generated ControlNet QR codes.
  - **High Performance:** Decodes 4K screens in ~40ms and 1080p screens in ~10ms.
- **Smart QR Code Recognition:**
  - **Wi-Fi QR Codes:** Automatically parses network name (SSID), security type, and password with a dedicated one-click **"Copy Pass"** button.
  - **Web URLs:** Direct **"Open"** in default browser alongside copy.
  - **Plain Text:** Monospaced card format with quick clipboard copy.
- **Output Logic:**
  - **0 QR codes:** Quiet tray notification ("No QR Code Found").
  - **1 QR code:**
    - *Default:* Interactive result dialog with direct copy (press `Enter` to copy, `Esc` to close).
    - *Optional (`Copy to clipboard (skip result window)`):* Window is skipped; content is copied directly to clipboard with a system tray notification.
  - **2+ QR codes:** Unified interactive result window with individual actions and a "Copy All" button.
- **Clean Native Desktop UI:**
  - Standard Windows utility aesthetics (neutral, native styling; no website hero banners or artificial AI slop).
  - Unified card design for both single and multiple QR scan results.
  - Custom hotkey selector with human-readable keys (`Page Down`, `Page Up`, `Enter`, `Delete`, `Tab`, arrow keys).
  - Configurable preferences: direct copy (skip window), capture sound, tray notifications, Windows startup.
  - Direct "Test Scan" button from settings.
- **Smart Portable:**
  - Detects if running from Downloads or temporary folders and offers one-click migration to `%LOCALAPPDATA%\Programs\QRSniper` with a Start Menu shortcut.
  - Functions 100% standalone even when moved alone to Desktop or USB drives.

---

## Download

Standalone executable releases are available on **[GitHub Releases](https://github.com/ngkaa0/qr-sniper/releases)**.  
No installer or dependencies required — simply download `QRSniper.exe` and run.

## Building from Source

No third-party SDKs or package managers required:
- Run **`build.bat`** (compiles in ~0.8s using Windows built-in `csc.exe`).

---

## Author & Credits

- **Author & Vision:** [ngkaa](https://github.com/ngkaa0)
- **Development:** 100% pure vibe coding with [Gemini](https://gemini.google.com/) in [Google Antigravity](https://antigravity.google/)
- **License:** MIT License
