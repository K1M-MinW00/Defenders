param(
    [string]$Source = 'Assets/Art/Concepts/PlagueAlchemist/PlagueAlchemist-Idle-Reference.png',
    [string]$Output = 'Assets/Art/Generated/Units/PlagueAlchemist/PlagueAlchemist-Move.png',
    [string]$Preview = ''
)

Add-Type -AssemblyName System.Drawing

$sourceBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source))
if ($sourceBitmap.Width -ne 100 -or $sourceBitmap.Height -ne 100) {
    $sourceBitmap.Dispose()
    throw 'The approved Idle reference must be exactly 100x100 pixels.'
}

function Copy-Frame([System.Drawing.Bitmap]$sheet, [int]$frameIndex) {
    for ($y = 0; $y -lt 100; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $sheet.SetPixel(($frameIndex * 100) + $x, $y, $sourceBitmap.GetPixel($x, $y))
        }
    }
}

function Shift-Region(
    [System.Drawing.Bitmap]$sheet,
    [int]$frameIndex,
    [System.Drawing.Rectangle]$region,
    [int]$dx,
    [int]$dy
) {
    if ($dx -eq 0 -and $dy -eq 0) { return }
    $cellX = $frameIndex * 100
    $pixels = [System.Collections.Generic.List[object]]::new()
    for ($y = $region.Top; $y -lt $region.Bottom; $y++) {
        for ($x = $region.Left; $x -lt $region.Right; $x++) {
            $pixel = $sheet.GetPixel($cellX + $x, $y)
            if ($pixel.A -ne 0) {
                $pixels.Add([pscustomobject]@{ X = $x; Y = $y; Color = $pixel })
            }
            $sheet.SetPixel($cellX + $x, $y, [System.Drawing.Color]::Transparent)
        }
    }
    foreach ($item in $pixels) {
        $targetX = $item.X + $dx
        $targetY = $item.Y + $dy
        if ($targetX -ge 0 -and $targetX -lt 100 -and $targetY -ge 0 -and $targetY -lt 100) {
            $sheet.SetPixel($cellX + $targetX, $targetY, $item.Color)
        }
    }
}

function Shift-UpperBody([System.Drawing.Bitmap]$sheet, [int]$frameIndex, [int]$dy) {
    if ($dy -eq 0) { return }
    $cellX = $frameIndex * 100
    $seamY = 47
    $pixels = [System.Collections.Generic.List[object]]::new()
    for ($y = 0; $y -le $seamY; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $pixel = $sheet.GetPixel($cellX + $x, $y)
            if ($pixel.A -ne 0) { $pixels.Add([pscustomobject]@{ X = $x; Y = $y; Color = $pixel }) }
            $sheet.SetPixel($cellX + $x, $y, [System.Drawing.Color]::Transparent)
        }
    }
    foreach ($item in $pixels) {
        $targetY = $item.Y + $dy
        if ($targetY -ge 0 -and $targetY -lt 100) {
            $sheet.SetPixel($cellX + $item.X, $targetY, $item.Color)
        }
    }
}

$sheet = [System.Drawing.Bitmap]::new(800, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for ($frame = 0; $frame -lt 8; $frame++) { Copy-Frame $sheet $frame }

# Only the lower boot clusters move. Keeping the upper legs and hips untouched
# prevents the one-pixel gaps that read as severed limbs at this resolution.
$leftLeg = [System.Drawing.Rectangle]::new(43, 51, 8, 5)
$rightLeg = [System.Drawing.Rectangle]::new(51, 51, 8, 5)
$poses = @(
    @(-1, 1),
    @(-1, 0),
    @(0, 0),
    @(1, -1),
    @(1, -1),
    @(0, -1),
    @(0, 0),
    @(-1, 1)
)

for ($frame = 0; $frame -lt 8; $frame++) {
    $pose = $poses[$frame]
    Shift-Region $sheet $frame $leftLeg $pose[0] 0
    Shift-Region $sheet $frame $rightLeg $pose[1] 0
}

$outputDirectory = [System.IO.Path]::GetDirectoryName($Output)
if (-not [System.IO.Directory]::Exists($outputDirectory)) { [void][System.IO.Directory]::CreateDirectory($outputDirectory) }
$sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

if ($Preview) {
    $previewBitmap = [System.Drawing.Bitmap]::new(3200, 400, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $previewGraphics = [System.Drawing.Graphics]::FromImage($previewBitmap)
    $previewGraphics.Clear([System.Drawing.Color]::FromArgb(255, 95, 68, 56))
    $previewGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $previewGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    $previewGraphics.DrawImage($sheet, [System.Drawing.Rectangle]::new(0, 0, 3200, 400), [System.Drawing.Rectangle]::new(0, 0, 800, 100), [System.Drawing.GraphicsUnit]::Pixel)
    $previewGraphics.Dispose()
    $previewDirectory = [System.IO.Path]::GetDirectoryName($Preview)
    if (-not [System.IO.Directory]::Exists($previewDirectory)) { [void][System.IO.Directory]::CreateDirectory($previewDirectory) }
    $previewBitmap.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
    $previewBitmap.Dispose()
}

$sheet.Dispose()
$sourceBitmap.Dispose()
Write-Output 'Created 800x100 eight-frame Move sheet from the approved reference.'
