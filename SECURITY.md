# Security Policy — Dastavez Studio

## Supported Versions
| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Offline Guarantee & Threat Model
Dastavez Studio processes sensitive applicant identity documents, photographs, signatures, and certificates.
To protect users:
1. **Network Boundary:** The application is architected to perform 100% of processing offline. Outbound sockets are not initiated by document processing code.
2. **Data Retention:** Input files are opened read-only. Intermediate bitmaps or byte arrays are disposed following standard `IDisposable` patterns.
3. **No Credential Phishing:** The application does not request, store, or transmit exam login credentials or personal passwords.
4. **Third-Party Library Auditing:** All binary and NuGet dependencies are audited for permissive commercial/redistribution licensing and known CVE vulnerabilities.

## Reporting a Vulnerability
If you discover a security vulnerability or network leak within Dastavez Studio, please open an issue or contact security@ilovedastavez.local.
