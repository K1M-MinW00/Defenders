param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$palette = @(
    '#1F1A1A', '#4C2D27', '#75483A', '#9A6550',
    '#292626', '#4C4541',
    '#3C3D25', '#66652F', '#928B42',
    '#6A4A22', '#A77A2E', '#E2B84A',
    '#59602A', '#91953A', '#D2D85B',
    '#B7B19A', '#EEEBD3'
) | ForEach-Object { [System.Drawing.ColorTranslator]::FromHtml($_) }

function Get-NearestPaletteColor([System.Drawing.Color]$color) {
    $best = $palette[0]
    $bestDistance = [double]::MaxValue
    foreach ($candidate in $palette) {
        $dr = [int]$color.R - [int]$candidate.R
        $dg = [int]$color.G - [int]$candidate.G
        $db = [int]$color.B - [int]$candidate.B
        $distance = (2 * $dr * $dr) + (4 * $dg * $dg) + (3 * $db * $db)
        if ($distance -lt $bestDistance) {
            $bestDistance = $distance
            $best = $candidate
        }
    }
    return [System.Drawing.Color]::FromArgb(255, $best.R, $best.G, $best.B)
}

$sourceBitmap = [System.Drawing.Bitmap]::FromFile($Source)
$minX = $sourceBitmap.Width
$minY = $sourceBitmap.Height
$maxX = -1
$maxY = -1

for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
    for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
        if ($sourceBitmap.GetPixel($x, $y).A -ge 200) {
            if ($x -lt $minX) { $minX = $x }
            if ($x -gt $maxX) { $maxX = $x }
            if ($y -lt $minY) { $minY = $y }
            if ($y -gt $maxY) { $maxY = $y }
        }
    }
}

if ($maxX -lt $minX -or $maxY -lt $minY) {
    $sourceBitmap.Dispose()
    throw 'The source image contains no opaque artwork.'
}

$crop = [System.Drawing.Rectangle]::new($minX, $minY, $maxX - $minX + 1, $maxY - $minY + 1)
$targetHeight = 23
$targetWidth = 22
$targetX = [Math]::Floor((100 - $targetWidth) / 2)
$targetY = 56 - $targetHeight

$canvas = [System.Drawing.Bitmap]::new(100, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$graphics.Clear([System.Drawing.Color]::Transparent)
$graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
$graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
$graphics.DrawImage($sourceBitmap, [System.Drawing.Rectangle]::new($targetX, $targetY, $targetWidth, $targetHeight), $crop, [System.Drawing.GraphicsUnit]::Pixel)
$graphics.Dispose()
$sourceBitmap.Dispose()

for ($y = 0; $y -lt 100; $y++) {
    for ($x = 0; $x -lt 100; $x++) {
        $color = $canvas.GetPixel($x, $y)
        if ($color.A -lt 200) {
            $canvas.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
        } else {
            # The source portrait contains many intermediate shadows. Lift them before
            # palette reduction so the 20x26 gameplay silhouette stays readable beside
            # the project's Archer/Priest/Wizard sprites.
            $lifted = [System.Drawing.Color]::FromArgb(
                255,
                [Math]::Min(255, ([int]$color.R + 10)),
                [Math]::Min(255, ([int]$color.G + 10)),
                [Math]::Min(255, ([int]$color.B + 8))
            )
            $canvas.SetPixel($x, $y, (Get-NearestPaletteColor $lifted))
        }
    }
}

# Match the purchased sheets: every exposed contour pixel uses the shared
# one-pixel #1F1A1A boundary color.
$outline = [System.Drawing.Color]::FromArgb(255, 31, 26, 26)
$edgePoints = [System.Collections.Generic.List[System.Drawing.Point]]::new()
for ($y = 0; $y -lt 100; $y++) {
    for ($x = 0; $x -lt 100; $x++) {
        if ($canvas.GetPixel($x, $y).A -eq 0) { continue }
        foreach ($offset in @(@(-1,0), @(1,0), @(0,-1), @(0,1))) {
            $nx = $x + $offset[0]
            $ny = $y + $offset[1]
            if ($nx -lt 0 -or $nx -ge 100 -or $ny -lt 0 -or $ny -ge 100 -or $canvas.GetPixel($nx, $ny).A -eq 0) {
                $edgePoints.Add([System.Drawing.Point]::new($x, $y))
                break
            }
        }
    }
}
foreach ($point in $edgePoints) { $canvas.SetPixel($point.X, $point.Y, $outline) }

$outputDirectory = [System.IO.Path]::GetDirectoryName($Output)
if (-not [System.IO.Directory]::Exists($outputDirectory)) { [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null }
$canvas.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

$previewBitmap = [System.Drawing.Bitmap]::new(800, 800, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$previewGraphics = [System.Drawing.Graphics]::FromImage($previewBitmap)
$previewGraphics.Clear([System.Drawing.Color]::FromArgb(255, 91, 68, 58))
$previewGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$previewGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
$previewGraphics.DrawImage($canvas, [System.Drawing.Rectangle]::new(0, 0, 800, 800), [System.Drawing.Rectangle]::new(0, 0, 100, 100), [System.Drawing.GraphicsUnit]::Pixel)
$previewGraphics.Dispose()
$previewDirectory = [System.IO.Path]::GetDirectoryName($Preview)
if (-not [System.IO.Directory]::Exists($previewDirectory)) { [System.IO.Directory]::CreateDirectory($previewDirectory) | Out-Null }
$previewBitmap.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
$previewBitmap.Dispose()
$canvas.Dispose()

Write-Output ("Created 100x100 idle reference; opaque target={0}x{1}, x={2}, baselineY=55" -f $targetWidth, $targetHeight, $targetX)
