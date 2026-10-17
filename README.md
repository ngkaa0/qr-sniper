# QR Sniper

> **Version:** `v0.9.6`  
> Ultra-lightweight, 100% standalone on-screen QR code scanner for clean Windows 10 & 11 installations.

---

## Performance & Footprint

| Metric | Value |
|---|---|
| **Binary Size** | **~635 KB** (single standalone `QRSniper.exe`, ZXing engine and icons embedded) |
| **Idle Memory (RAM)** | **~3–5 MB** (with native Working Set trim) |
| **Idle CPU** | **0.0%** (native OS hooks via `RegisterHotKey`) |
| **Clean OS Support** | **Runs out-of-the-box on clean Windows 10/11 without extra runtimes** |

---

## Features

- **Instant Full-Screen Scan:**
  Press your global shortcut (default: `Ctrl + Shift + Q`) to capture and decode all QR codes on screen across multi-monitor setups and high-DPI displays.
- **Output Logic:**
  - **0 QR codes:** Quiet tray notification ("No QR Code Found").
  - **1 QR code:**
    - *Default (checkbox unchecked):* Interactive unified result window opens with the decoded card (`Copy` and `Open URL` buttons). Content is not copied to clipboard automatically.
    - *Optional (`Copy to clipboard (skip result window)` checked):* Window is skipped; content is copied directly to clipboard with a system tray notification.
  - **2+ QR codes:** Unified interactive result window with direct copy and browser links for each detected item.
- **Clean Native Desktop UI:**
  - Standard Windows utility aesthetics (neutral, native styling; no website hero banners or artificial AI slop).
  - Unified card design for both single and multiple QR scan results.
  - Custom hotkey selector with human-readable keys (`Page Down`, `Page Up`, `Enter`, `Delete`, `Tab`, arrow keys).
  - Configurable preferences: direct copy (skip window), capture sound, tray notifications, Windows startup.
  - Direct "Test Scan" button from settings.
- **Smart Portable:**
  - Detects if running from Downloads or temporary folders and offers one-click migration to `%LOCALAPPDATA%\Programs\QRSniper` with a Start Menu shortcut.
  - Functions 100% standalone even when moved alone to Desktop or USB drives.
- **Version Archive:**
  - Built-in versioning archives past builds in `versions/` (e.g. `versions/v0.9.6/`).

---

## Building from Source

No third-party SDKs or package managers required:
- Run **`build.bat`** (compiles in ~0.8s using Windows built-in `csc.exe`).

---

## Author & Credits

- **Author & Vision:** [ngkaa](https://github.com/ngkaa0)
- **Development:** 100% pure vibe coding with [Gemini](https://gemini.google.com/) in [Google Antigravity](https://antigravity.google/)
- **License:** MIT License
