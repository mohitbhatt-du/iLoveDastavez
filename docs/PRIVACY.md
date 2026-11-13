# Privacy Policy & Security Architecture — Dastavez Studio

**Brand:** iLoveDastavez  
**Application:** Dastavez Studio  
**Tagline:** हर दस्तावेज़, तैयार!  

---

## 1. Zero Cloud & Local Isolation
Dastavez Studio operates under an uncompromising **offline-first and zero-knowledge model**:
- **No Cloud Uploads:** No file, photo, signature, text, or document is ever sent to any remote server or third-party service.
- **No Network Activity:** The core application operates in complete network isolation. It does not require an active internet connection to execute any processing tasks.
- **No Analytics / Telemetry:** There are no embedded tracking SDKs, telemetry monitors, diagnostic reporting tools, or analytics beacons.

## 2. Document Processing & In-Memory Lifecycle
1. **Source Protection:** Input files are treated as read-only. The application never modifies or overwrites the source file unless the user explicitly chooses to replace it during an export dialog.
2. **Ephemeral Memory Processing:** Image decoding, resizing, compression, and PDF operations are handled in-memory whenever possible.
3. **Temporary File Sanitization:** When disk-backed storage is strictly required for large file conversions:
   * Files are written to an isolated, randomized temporary directory (`%TEMP%\DastavezStudio\Session_<Guid>`).
   * Temporary files are purged immediately upon operation completion, task cancellation, or application exit.
4. **No History Tracking:** The application logs recently used *tools* (e.g., "Compress PDF", "IBPS Preset"), but **never** retains document names, paths, thumbnails, or sensitive text extracts.

## 3. Local AI & OCR Inference
- **ONNX Runtime:** All AI-based image enhancement and upscaling models are stored locally on the user's hard drive. No external API calls are made.
- **Tesseract OCR:** OCR language packs are stored and executed strictly on the local machine. Extracted text is output only directly to the user or into the user's exported searchable PDF.

## 4. Verification & Auditing
Users and security researchers can verify network isolation using tools such as Windows Resource Monitor, Sysinternals TCPView, or Wireshark. Under normal operation, Dastavez Studio makes zero outbound socket connections.
