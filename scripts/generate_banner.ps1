Add-Type -AssemblyName System.Drawing

$w = 680
$h = 160
$scale = 2

$bw = $w * $scale
$bh = $h * $scale

$bmp = New-Object System.Drawing.Bitmap($bw, $bh, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$g.Clear([System.Drawing.Color]::Transparent)

# 1. Background Rounded Card
$rx = [float](4 * $scale)
$ry = [float](4 * $scale)
$rw = [float](($w - 8) * $scale)
$rh = [float](($h - 8) * $scale)
$rad = [float](16 * $scale)
$diam = [float]($rad * 2.0)

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddArc($rx, $ry, $diam, $diam, 180.0, 90.0)
$path.AddArc([float]($rx + $rw - $diam), $ry, $diam, $diam, 270.0, 90.0)
$path.AddArc([float]($rx + $rw - $diam), [float]($ry + $rh - $diam), $diam, $diam, 0.0, 90.0)
$path.AddArc($rx, [float]($ry + $rh - $diam), $diam, $diam, 90.0, 90.0)
$path.CloseFigure()

# Background color (#0B0F19 deep dark obsidian)
$bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 11, 15, 25))
$g.FillPath($bgBrush, $path)
$bgBrush.Dispose()

# Border stroke (#334155 metallic slate)
$borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 51, 65, 85), [float](2.0 * $scale))
$g.DrawPath($borderPen, $path)
$borderPen.Dispose()
$path.Dispose()

# 2. Draw Master Aesthetic Logo
$logoPath = "C:\Users\mohit\ai_projects\DastavezStudio\src\DastavezStudio.App\Assets\AppLogo.png"
if (Test-Path $logoPath) {
    $logo = [System.Drawing.Image]::FromFile($logoPath)
    $logoSize = [float](112 * $scale)
    $logoX = [float](26 * $scale)
    $logoY = [float]((($h * $scale) - $logoSize) / 2.0)
    $g.DrawImage($logo, $logoX, $logoY, $logoSize, $logoSize)
    $logo.Dispose()
}

# 3. Typography
$textX = [float](158 * $scale)
$fontTitle = New-Object System.Drawing.Font("Segoe UI", [float](34 * $scale), [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$fontSub = New-Object System.Drawing.Font("Segoe UI", [float](18 * $scale), [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
$fontTagline = New-Object System.Drawing.Font("Nirmala UI", [float](16 * $scale), [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)

$brushAccent = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 107, 0)) # #FF6B00
$brushWhite = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
$brushSub = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 148, 163, 184)) # #94A3B8
$brushTagline = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 56, 189, 248)) # #38BDF8

# Title
$titleY = [float](26 * $scale)
$g.DrawString("iLove", $fontTitle, $brushAccent, $textX, $titleY)
$sizeILove = $g.MeasureString("iLove", $fontTitle)
$dastavezX = [float]($textX + $sizeILove.Width - (10.0 * $scale))
$g.DrawString("Dastavez", $fontTitle, $brushWhite, $dastavezX, $titleY)

# Subtitle
$subY = [float](74 * $scale)
$g.DrawString("Dastavez Studio", $fontSub, $brushSub, [float]($textX + (2 * $scale)), $subY)

# Tagline with Nirmala UI (Hindi Devanagari + Latin)
$taglineY = [float](106 * $scale)
$g.DrawString("हर दस्तावेज़, तैयार! — 100% Offline", $fontTagline, $brushTagline, [float]($textX + (2 * $scale)), $taglineY)

# Cleanup
$fontTitle.Dispose()
$fontSub.Dispose()
$fontTagline.Dispose()
$brushAccent.Dispose()
$brushWhite.Dispose()
$brushSub.Dispose()
$brushTagline.Dispose()
$g.Dispose()

# Downsample smoothly to 680x160 for crisp Retina look
$finalBmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$fg = [System.Drawing.Graphics]::FromImage($finalBmp)
$fg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$fg.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$fg.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$fg.DrawImage($bmp, 0, 0, $w, $h)
$fg.Dispose()
$bmp.Dispose()

$dest1 = "C:\Users\mohit\ai_projects\DastavezStudio\assets\brand_banner.png"
$dest2 = "C:\Users\mohit\ai_projects\DastavezStudio\src\DastavezStudio.App\Assets\Logos\brand_banner.png"

$finalBmp.Save($dest1, [System.Drawing.Imaging.ImageFormat]::Png)
$finalBmp.Save($dest2, [System.Drawing.Imaging.ImageFormat]::Png)
$finalBmp.Dispose()

Write-Output "Brand banner generated successfully with Nirmala UI!"
