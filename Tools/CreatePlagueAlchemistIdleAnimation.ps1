param(
    [string]$Source = 'Assets/Art/Concepts/PlagueAlchemist/PlagueAlchemist-Idle-Reference.png',
    [string]$Output = 'Assets/Art/Generated/Units/PlagueAlchemist/PlagueAlchemist-Idle.png',
    [string]$Preview = ''
)

Add-Type -AssemblyName System.Drawing

$sourceBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source))
if ($sourceBitmap.Width -ne 100 -or $sourceBitmap.Height -ne 100) {
    $sourceBitmap.Dispose()
    throw 'The approved Idle reference must be exactly 100x100 pixels.'
}

function Copy-BaseFrame([System.Drawing.Bitmap]$sheet, [int]$frameIndex) {
    for ($y = 0; $y -lt 100; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $sheet.SetPixel(($frameIndex * 100) + $x, $y, $sourceBitmap.GetPixel($x, $y))
        }
    }
}

function Apply-UpperBreath([System.Drawing.Bitmap]$sheet, [int]$frameIndex, [int]$seamY, [int]$direction) {
    $cellX = $frameIndex * 100
    $snapshot = [System.Drawing.Bitmap]::new(100, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt 100; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $snapshot.SetPixel($x, $y, $sheet.GetPixel($cellX + $x, $y))
        }
    }

    # Clear only the upper-body zone. The legs and the y=55 foot baseline never move.
    for ($y = 0; $y -le $seamY; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $sheet.SetPixel($cellX + $x, $y, [System.Drawing.Color]::Transparent)
        }
    }

    for ($y = 0; $y -le $seamY; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $pixel = $snapshot.GetPixel($x, $y)
            if ($pixel.A -eq 0) { continue }
            $targetY = $y + $direction
            if ($targetY -ge 0 -and $targetY -lt 100) {
                $sheet.SetPixel($cellX + $x, $targetY, $pixel)
            }
        }
    }

    # Restore the seam row to keep coat/arm connections intact instead of creating
    # a transparent crack or duplicating independently generated body parts.
    for ($x = 0; $x -lt 100; $x++) {
        $pixel = $snapshot.GetPixel($x, $seamY)
        if ($pixel.A -ne 0) { $sheet.SetPixel($cellX + $x, $seamY, $pixel) }
    }

    $snapshot.Dispose()
}

$sheet = [System.Drawing.Bitmap]::new(600, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for ($frame = 0; $frame -lt 6; $frame++) { Copy-BaseFrame $sheet $frame }

# Purchased-sheet cadence: base -> small inhale -> full inhale -> small inhale
# -> base -> soft exhale. Changes are limited to one integer pixel.
Apply-UpperBreath $sheet 1 40 -1
Apply-UpperBreath $sheet 2 44 -1
Apply-UpperBreath $sheet 3 40 -1
Apply-UpperBreath $sheet 5 40 1

$outputDirectory = [System.IO.Path]::GetDirectoryName($Output)
if (-not [System.IO.Directory]::Exists($outputDirectory)) {
    [void][System.IO.Directory]::CreateDirectory($outputDirectory)
}
$sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

if ($Preview) {
    $previewBitmap = [System.Drawing.Bitmap]::new(2400, 400, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $previewGraphics = [System.Drawing.Graphics]::FromImage($previewBitmap)
    $previewGraphics.Clear([System.Drawing.Color]::FromArgb(255, 95, 68, 56))
    $previewGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $previewGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    $previewGraphics.DrawImage($sheet, [System.Drawing.Rectangle]::new(0, 0, 2400, 400), [System.Drawing.Rectangle]::new(0, 0, 600, 100), [System.Drawing.GraphicsUnit]::Pixel)
    $previewGraphics.Dispose()
    $previewDirectory = [System.IO.Path]::GetDirectoryName($Preview)
    if (-not [System.IO.Directory]::Exists($previewDirectory)) {
        [void][System.IO.Directory]::CreateDirectory($previewDirectory)
    }
    $previewBitmap.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
    $previewBitmap.Dispose()
}

$sheet.Dispose()
$sourceBitmap.Dispose()
Write-Output 'Created 600x100 six-frame Idle sheet from the approved reference.'
