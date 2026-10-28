param(
    [string]$Source = 'Assets/Art/Concepts/PlagueAlchemist/PlagueAlchemist-Idle-Reference.png',
    [string]$OutputDirectory = 'Assets/Art/Generated/Units/PlagueAlchemist',
    [string]$PreviewDirectory = ''
)

Add-Type -AssemblyName System.Drawing
$sourceBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source))
if ($sourceBitmap.Width -ne 100 -or $sourceBitmap.Height -ne 100) {
    $sourceBitmap.Dispose()
    throw 'The approved Idle reference must be exactly 100x100 pixels.'
}

function Copy-SourceCell([System.Drawing.Bitmap]$sheet, [int]$frameIndex) {
    for ($y = 0; $y -lt 100; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $sheet.SetPixel(($frameIndex * 100) + $x, $y, $sourceBitmap.GetPixel($x, $y))
        }
    }
}

function Shift-AttackArm([System.Drawing.Bitmap]$sheet, [int]$frameIndex, [int]$dx, [int]$dy) {
    if ($dx -eq 0 -and $dy -eq 0) { return }
    $cellX = $frameIndex * 100
    $region = [System.Drawing.Rectangle]::new(39, 42, 10, 7)
    $pixels = [System.Collections.Generic.List[object]]::new()
    for ($y = $region.Top; $y -lt $region.Bottom; $y++) {
        for ($x = $region.Left; $x -lt $region.Right; $x++) {
            $pixel = $sheet.GetPixel($cellX + $x, $y)
            if ($pixel.A -ne 0) { $pixels.Add([pscustomobject]@{ X=$x; Y=$y; Color=$pixel }) }
            $sheet.SetPixel($cellX + $x, $y, [System.Drawing.Color]::Transparent)
        }
    }
    foreach ($item in $pixels) {
        $sheet.SetPixel($cellX + $item.X + $dx, $item.Y + $dy, $item.Color)
    }

    # Preserve the shoulder seam so the moved forearm never becomes a floating
    # component. These pixels are copied from the approved reference unchanged.
    for ($y = 43; $y -le 47; $y++) {
        for ($x = 47; $x -le 49; $x++) {
            $pixel = $sourceBitmap.GetPixel($x, $y)
            if ($pixel.A -ne 0) { $sheet.SetPixel($cellX + $x, $y, $pixel) }
        }
    }
}

function Make-AttackSheet {
    $sheet = [System.Drawing.Bitmap]::new(700, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($frame = 0; $frame -lt 7; $frame++) { Copy-SourceCell $sheet $frame }
    $poses = @(@(0,0), @(0,-1), @(1,-2), @(2,-2), @(1,-1), @(0,0), @(0,0))
    for ($frame = 0; $frame -lt 7; $frame++) {
        Shift-AttackArm $sheet $frame $poses[$frame][0] $poses[$frame][1]
    }
    return $sheet
}

function Make-DieSheet {
    $sheet = [System.Drawing.Bitmap]::new(400, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    Copy-SourceCell $sheet 0

    # Frame 2: one-pixel forward sag while both boots stay planted.
    Copy-SourceCell $sheet 1
    for ($y = 33; $y -le 48; $y++) {
        for ($x = 30; $x -lt 70; $x++) {
            $sheet.SetPixel(100 + $x, $y, [System.Drawing.Color]::Transparent)
        }
    }
    for ($y = 33; $y -le 48; $y++) {
        for ($x = 30; $x -lt 70; $x++) {
            $pixel = $sourceBitmap.GetPixel($x, $y)
            if ($pixel.A -ne 0) { $sheet.SetPixel(100 + $x + 1, $y + 1, $pixel) }
        }
    }

    # Frame 3: deeper collapse toward screen-right, still connected to the legs.
    Copy-SourceCell $sheet 2
    for ($y = 33; $y -le 50; $y++) {
        for ($x = 30; $x -lt 70; $x++) { $sheet.SetPixel(200 + $x, $y, [System.Drawing.Color]::Transparent) }
    }
    for ($y = 33; $y -le 50; $y++) {
        $rowShiftX = if ($y -lt 41) { 3 } elseif ($y -lt 47) { 2 } else { 1 }
        $rowShiftY = if ($y -lt 47) { 3 } else { 2 }
        for ($x = 30; $x -lt 70; $x++) {
            $pixel = $sourceBitmap.GetPixel($x, $y)
            if ($pixel.A -ne 0) { $sheet.SetPixel(200 + $x + $rowShiftX, $y + $rowShiftY, $pixel) }
        }
    }

    # Frame 4: exact 90-degree clockwise pixel rotation of the approved sprite,
    # placed flat on the same y=55 ground line. No interpolation is used.
    $cropX = 39; $cropY = 33; $cropW = 22; $cropH = 23
    $finalLeft = 38; $finalTop = 34
    for ($sy = 0; $sy -lt $cropH; $sy++) {
        for ($sx = 0; $sx -lt $cropW; $sx++) {
            $pixel = $sourceBitmap.GetPixel($cropX + $sx, $cropY + $sy)
            if ($pixel.A -eq 0) { continue }
            $dx = $cropH - 1 - $sy
            $dy = $sx
            $sheet.SetPixel(300 + $finalLeft + $dx, $finalTop + $dy, $pixel)
        }
    }
    return $sheet
}

function Save-SheetAndPreview([System.Drawing.Bitmap]$sheet, [string]$fileName) {
    if (-not [System.IO.Directory]::Exists($OutputDirectory)) { [void][System.IO.Directory]::CreateDirectory($OutputDirectory) }
    $outputPath = Join-Path $OutputDirectory $fileName
    $sheet.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    if ($PreviewDirectory) {
        if (-not [System.IO.Directory]::Exists($PreviewDirectory)) { [void][System.IO.Directory]::CreateDirectory($PreviewDirectory) }
        $preview = [System.Drawing.Bitmap]::new($sheet.Width * 4, 400, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($preview)
        $graphics.Clear([System.Drawing.Color]::FromArgb(255, 95, 68, 56))
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.DrawImage($sheet, [System.Drawing.Rectangle]::new(0, 0, $sheet.Width * 4, 400), [System.Drawing.Rectangle]::new(0, 0, $sheet.Width, 100), [System.Drawing.GraphicsUnit]::Pixel)
        $graphics.Dispose()
        $preview.Save((Join-Path $PreviewDirectory ($fileName -replace '\.png$', '-preview.png')), [System.Drawing.Imaging.ImageFormat]::Png)
        $preview.Dispose()
    }
}

$attack = Make-AttackSheet
Save-SheetAndPreview $attack 'PlagueAlchemist-Attack.png'
$attack.Dispose()

$die = Make-DieSheet
Save-SheetAndPreview $die 'PlagueAlchemist-Die.png'
$die.Dispose()
$sourceBitmap.Dispose()

Write-Output 'Created Attack (7 frames) and Die (4 frames) sheets.'
