# Dastavez Studio (iLoveDastavez)
> **हर दस्तावेज़, तैयार!** — Complete Offline Document & Image Toolkit for Windows

[![Windows 10/11 x64](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011%20(x64)-blue.svg)](#system-requirements)
[![Offline First](https://img.shields.io/badge/Privacy-100%25%20Offline-success.svg)](#privacy-guarantee)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📌 Introduction

**Dastavez Studio** by **iLoveDastavez** is a Windows desktop application engineered for students, competitive exam aspirants (UPSC, SSC, IBPS, SBI, RRB, NTA, State PSCs), job applicants, and everyday users.

Applying for exams in India typically involves navigating strict, frustrating upload criteria:
* Exact pixel dimensions (e.g., 200 × 230 px for photos, 140 × 60 px for signatures)
* Strict file size ceilings (e.g., between 20 KB and 50 KB)
* Specific aspect ratios, formats (JPG/JPEG), and DPI requirements

Dastavez Studio handles these tasks locally on your device with verified presets, target-size compression, and privacy protection.

---

## 🔒 Privacy Guarantee

1. **100% Offline Processing:** All operations run locally on your PC. No internet connection is ever required for document operations.
2. **Zero Cloud Uploads:** Your photos, signatures, identity proofs, and certificates never leave your machine.
3. **No Accounts or Logins:** Open and use immediately without registration.
4. **No Telemetry / Tracking:** No user tracking, analytics, or behavioral collection.
5. **Memory-First / Ephemeral Storage:** Files are processed in memory whenever possible. Any temporary cache files are automatically purged.
6. **Non-Destructive:** Your original source documents are never overwritten without explicit confirmation.

---

## ✨ Feature Highlights

### 🎓 Exam Document Studio (Signature Feature)
* **Built-in Presets:** Pre-configured rules for IBPS, SBI, SSC, UPSC, RRB, NTA, and State PSCs.
* **Document Types:** Passport Photographs, Signatures, Left Thumb Impressions (LTI), Handwritten Declarations, and Educational/Caste/Income Certificates.
* **Live Compliance Validator:** Real-time visual feedback (Green = Compliant, Amber = Needs Attention, Red = Non-compliant).
* **Custom Presets:** Create and save your own exam requirements.

### 🖼️ Image Toolkit
* **Smart Compression:** Specify an exact target size (e.g., 50 KB) with iterative compression.
* **Precision Resizing:** Resize by pixels, percentage, or physical dimensions (cm/mm/inches) while preserving aspect ratio.
* **Editing Suite:** Crop, rotate, flip, adjust brightness/contrast, sharpen, and convert to grayscale.
* **Metadata & Privacy:** Strip EXIF metadata and adjust DPI headers.
* **Formats Supported:** JPEG, PNG, WEBP, BMP, TIFF.

### 📄 PDF Toolkit
* **Compress PDF:** Target-size and level-based compression while preserving searchable text and bookmarks.
* **Page Management:** Merge, split, reorder, rotate, extract, and delete pages.
* **Image Conversion:** High-fidelity conversion between PDF pages and JPG/PNG.
* **Security:** Add password protection and authorized password removal.

### ⚡ Batch Processing
* Bulk resize and compress dozens of files simultaneously with bounded CPU/memory concurrency.

### 🔍 Scanner & Offline OCR
* Document edge detection, deskew, perspective correction, shadow removal, and black-and-white enhancement.
* Offline OCR powered by Tesseract to generate searchable PDFs without internet access.

### 🤖 Local AI Enhancement (Optional)
* Offline 2x / 4x super-resolution and denoising powered by ONNX Runtime (CPU/GPU acceleration).

---

## 💻 System Requirements

* **Operating System:** Windows 10 (version 1809+ / Build 17763+) or Windows 11 (64-bit)
* **Architecture:** x64
* **Runtime:** .NET 9.0 / 10.0 Windows Desktop Runtime
* **Memory:** Minimum 4 GB RAM (8 GB recommended for batch / AI upscaling)
* **Disk Space:** ~150 MB for core application (~400 MB if local AI models are included)

---

## 🛠️ Technology Stack

| Layer | Component | Technology |
|---|---|---|
| **Language** | Primary | C# |
| **Framework** | Runtime | .NET (LTS) |
| **UI** | Interface | WinUI 3 / Windows App SDK (Fluent Design) |
| **Architecture** | Pattern | MVVM with `Microsoft.Extensions.DependencyInjection` |
| **Images** | 2D Graphics | SkiaSharp / SixLabors.ImageSharp |
| **PDF** | Operations & Rendering | PDFium / QPDF |
| **OCR** | Text Extraction | Offline Tesseract Engine |
| **AI Inference** | Local Neural Nets | ONNX Runtime |
| **Installer** | Packaging | Inno Setup & Portable ZIP |

---

## 🚀 Building from Source

### Prerequisites
1. Windows 10/11 x64
2. [.NET SDK](https://dotnet.microsoft.com/download)
3. Visual Studio 2022 / Build Tools with *.NET Desktop Development* and *Windows App SDK C# Templates*

### Build Steps
```powershell
# Clone the repository
git clone https://github.com/iLoveDastavez/DastavezStudio.git
cd DastavezStudio

# Restore dependencies
dotnet restore

# Build in Release configuration
dotnet build -c Release

# Run Unit Tests
dotnet test tests/DastavezStudio.UnitTests
```

---

## 📄 License & Disclaimers

Distributed under the [MIT License](LICENSE).

> **Disclaimer:** Examination authority names and acronyms (such as IBPS, SBI, SSC, UPSC, RRB, NTA) are trademarks of their respective governing bodies. Dastavez Studio is an independent utility and is not affiliated with, endorsed by, or sponsored by any government agency or recruitment board. Preset specifications are provided for user convenience and must be cross-verified against official notifications.
