# Generates compliant Windows Multi-Resolution DIB + PNG ICO from source PNG
param(
    [string]$PngPath = "$PSScriptRoot\..\src\DastavezStudio.App\Assets\AppLogo.png",
    [string]$IcoPath = "$PSScriptRoot\..\src\DastavezStudio.App\Assets\AppIcon.ico"
)

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $PngPath)) {
    Write-Error "Source PNG not found at: $PngPath"
    exit 1
}

$srcImg = [System.Drawing.Image]::FromFile($PngPath)
$sizes = @(16, 32, 48, 64, 128, 256)
$entries = @()

foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.DrawImage($srcImg, 0, 0, $size, $size)
    $g.Dispose()

    if ($size -eq 256) {
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $bytes = $ms.ToArray()
        $ms.Dispose()
    } else {
        $ms = New-Object System.IO.MemoryStream
        $bw = New-Object System.IO.BinaryWriter($ms)

        $width = $size
        $height = $size
        $andRowBytes = [int][Math]::Floor(($width + 31) / 32) * 4
        $pixelDataSize = $width * $height * 4

        # BITMAPINFOHEADER (40 bytes)
        $bw.Write([uint32]40)
        $bw.Write([int32]$width)
        $bw.Write([int32]($height * 2))
        $bw.Write([uint16]1)
        $bw.Write([uint16]32)
        $bw.Write([uint32]0)
        $bw.Write([uint32]$pixelDataSize)
        $bw.Write([int32]0)
        $bw.Write([int32]0)
        $bw.Write([uint32]0)
        $bw.Write([uint32]0)

        # XOR Mask: 32bpp BGRA from bottom row to top row
        for ($y = $height - 1; $y -ge 0; $y--) {
            for ($x = 0; $x -lt $width; $x++) {
                $c = $bmp.GetPixel($x, $y)
                $bw.Write([byte]$c.B)
                $bw.Write([byte]$c.G)
                $bw.Write([byte]$c.R)
                $bw.Write([byte]$c.A)
            }
        }

        # AND Mask: 1bpp
        for ($y = $height - 1; $y -ge 0; $y--) {
            for ($x = 0; $x -lt $andRowBytes; $x++) {
                [byte]$maskByte = 0
                for ($bit = 0; $bit -lt 8; $bit++) {
                    $px = $x * 8 + $bit
                    if ($px -lt $width) {
                        $c = $bmp.GetPixel($px, $y)
                        if ($c.A -eq 0) {
                            $maskByte = $maskByte -bor (0x80 -shr $bit)
                        }
                    }
                }
                $bw.Write($maskByte)
            }
        }

        $bw.Flush()
        $bytes = $ms.ToArray()
        $bw.Dispose()
        $ms.Dispose()
    }

    $bmp.Dispose()
    $entries += ,@($size, $bytes)
}

$srcImg.Dispose()

# Write ICO
$fs = [System.IO.File]::Create($IcoPath)
$w = New-Object System.IO.BinaryWriter($fs)

# Header
$w.Write([uint16]0)
$w.Write([uint16]1)
$w.Write([uint16]$entries.Count)

$offset = 6 + (16 * $entries.Count)
foreach ($entry in $entries) {
    $s = $entry[0]
    $b = $entry[1]
    [byte]$dim = if ($s -ge 256) { 0 } else { $s }
    $w.Write($dim)
    $w.Write($dim)
    $w.Write([byte]0)
    $w.Write([byte]0)
    $w.Write([uint16]1)
    $w.Write([uint16]32)
    $w.Write([uint32]$b.Length)
    $w.Write([uint32]$offset)
    $offset += $b.Length
}

foreach ($entry in $entries) {
    $w.Write($entry[1])
}

$w.Flush()
$w.Dispose()
$fs.Dispose()
Write-Output "ICO successfully generated at $IcoPath"
