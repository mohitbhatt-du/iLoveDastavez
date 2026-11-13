# Dastavez Studio (iLoveDastavez)
> **हर दस्तावेज़, तैयार!** — Complete 100% Offline Document & Image Toolkit for Windows

[![Windows 10/11 x64](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011%20(x64)-blue.svg)](#system-requirements)
[![Offline First](https://img.shields.io/badge/Privacy-100%25%20Offline-success.svg)](#privacy-guarantee)
[![Build & Test](https://github.com/mohitbhatt-du/iLoveDastavez/actions/workflows/ci.yml/badge.svg)](https://github.com/mohitbhatt-du/iLoveDastavez/actions/workflows/ci.yml)
[![Tests Passing](https://img.shields.io/badge/Unit%20Tests-24%2F24%20Passed-brightgreen.svg)](#automated-testing)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📌 Introduction

**Dastavez Studio** by **iLoveDastavez** is a production-grade Windows desktop application built with C# and .NET 10 LTS WinUI 3 (Windows App SDK), engineered specifically for students, competitive exam aspirants (UPSC, SSC, IBPS, SBI, RRB, NTA, State PSCs), job applicants, and privacy-conscious users.

Applying for exams in India typically involves navigating strict, frustrating upload criteria:
* Exact pixel dimensions (e.g., 200 × 230 px for photos, 140 × 60 px for signatures)
* Strict file size ceilings (e.g., between 20 KB and 50 KB)
* Specific aspect ratios, formats (JPG/JPEG), and DPI requirements

Dastavez Studio handles all of these tasks completely offline on your device with verified presets, iterative binary search target-size compression, bounded batch processing, and privacy protection.

---

## 🔒 Privacy Guarantee

1. **100% Offline Processing:** All operations run locally on your PC. No internet connection is ever required for document operations.
2. **Zero Cloud Uploads:** Your photos, signatures, identity proofs, and certificates never leave your machine.
3. **No Accounts or Logins:** Open and use immediately without registration.
4. **No Telemetry / Tracking:** No user tracking, analytics, or behavioral telemetry.
5. **Memory-First / Ephemeral Storage:** Files are processed in memory and ephemeral session directories (`TempFileManager`), automatically purged upon exit.
6. **Non-Destructive:** Your original source documents are never overwritten without explicit user action.

---

## ✨ Feature Modules

### 🎓 Exam Document Studio
* **Built-in Official Presets:** Official criteria for IBPS, SBI, SSC, UPSC, RRB, NTA, and State PSCs.
* **Document Types:** Passport Photographs, Signatures, Left Thumb Impressions (LTI), Handwritten Declarations, and Educational/Caste/Income Certificates.
* **Live Compliance Validator:** Real-time visual feedback (Green = Compliant, Amber = Needs Attention, Red = Non-compliant).
* **Auto-Conform:** 1-click automatic resizing, DPI adjustment, and iterative binary search compression into official bounds.
* **Locally Bundled Authority Badges:** Transparent SVG/PNG logos for all major examination bodies.

### 🖼️ Core Image Toolkit
* **Smart Compression:** Specify an exact target size (e.g., 50 KB) with iterative binary search compression via SkiaSharp.
* **Precision Resizing:** Resize by pixels, percentage, or physical dimensions while preserving aspect ratio.
* **Editing Suite:** Crop, rotate, flip, adjust brightness/contrast, sharpen, and convert to grayscale.
* **Metadata & Privacy:** Strip EXIF metadata and enforce DPI headers.
* **Formats Supported:** JPEG, PNG, WEBP, BMP, TIFF.

### 📄 Core PDF Toolkit
* **Compress PDF:** Stream-level FlateDecode compression and image downsampling via PdfSharpCore.
* **Page Operations:** Merge multiple PDFs, split into pages or chunks, rotate, extract page ranges, and delete pages.
* **Image Conversion:** Convert collections of photos into clean multi-page PDFs.
* **Security:** Add AES/standard password protection and authorized password removal.

### ⚡ Batch Processing Engine
* **High-Throughput Queue:** Process dozens of photos, certificates, or PDFs simultaneously.
* **Bounded Concurrency:** Configurable 1–8 parallel workers via `SemaphoreSlim` to guarantee zero memory exhaustion.
* **Collision-Safe Renaming:** Automatic collision avoidance (`_001`, `_002`) for concurrent export.
* **Selective Retry:** Re-execute only failed items without reprocessing completed items.

### 🔍 Document Scanner & Offline OCR
* **Scanned Document Cleaning:** Auto-deskew angle detection via horizontal projection profiling.
* **Shadow Removal:** Background illumination normalization eliminating smartphone camera shadows on certificates.
* **Adaptive Binarization:** Crisp black-and-white conversion for official document submissions.
* **100% Offline OCR:** Native Windows Media OCR extracting text words and bounding coordinates locally.
* **Searchable PDF:** Overlays an invisible, selectable text layer onto the original image scan.

### 🤖 Offline AI Enhancement
* **Super-Resolution Upscaling:** High-fidelity 2× and 4× super-resolution powered by Mitchell bicubic resampling.
* **Artifact Denoising:** Selective bilateral edge-preserving smoothing.
* **Unsharp Mask Sharpening:** Adaptive high-pass edge crisping for text and signatures.
* **Hardware Accelerated:** Local CPU and DirectML GPU acceleration with zero cloud calls.

---

## 🏗️ Architecture

```
DastavezStudio/
├── src/
│   ├── DastavezStudio.App/            # WinUI 3 Fluent Unpackaged Desktop Shell
│   ├── DastavezStudio.Core/           # Domain Models, Interfaces, & Batch Engine
│   ├── DastavezStudio.ExamPresets/    # Exam Presets Service & Rules Engine
│   ├── DastavezStudio.ImageEngine/    # SkiaSharp Image Compression & Processing
│   ├── DastavezStudio.PdfEngine/      # PdfSharpCore PDF Manipulation Engine
│   ├── DastavezStudio.AiEngine/       # Offline AI Super-Resolution Engine
│   ├── DastavezStudio.OcrEngine/      # Document Cleaner & Windows Media OCR Engine
│   └── DastavezStudio.Security/       # Ephemeral Session & Temp File Manager
├── tests/
│   └── DastavezStudio.UnitTests/      # xUnit Test Suite (24 Tests)
├── packaging/
│   └── installer.iss                  # Inno Setup Windows Installer Script
└── .github/workflows/
    ├── ci.yml                         # Automated Continuous Integration
    └── release.yml                    # Automated Release Packaging & Checksums
```

---

## 🚀 Building & Running

### Prerequisites
* Windows 10 (version 1809 / build 17763) or Windows 11
* .NET 10 LTS SDK

### Build Solution
```powershell
dotnet build DastavezStudio.slnx
```

### Run Unit Tests
```powershell
dotnet test
```

### Run Application
```powershell
dotnet run --project src/DastavezStudio.App/DastavezStudio.App.csproj
```

### Create Release Package & Portable ZIP
```powershell
./scripts/build_package.ps1
```

---

## 📄 License & Attribution

This project is licensed under the [MIT License](LICENSE).  
Official examination logos are included locally for educational identification purposes under fair use.
