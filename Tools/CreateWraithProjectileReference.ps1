param(
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$outline = [System.Drawing.ColorTranslator]::FromHtml('#1F1A1A')
$darkTeal = [System.Drawing.ColorTranslator]::FromHtml('#425C6B')
$turquoise = [System.Drawing.ColorTranslator]::FromHtml('#607581')
$mint = [System.Drawing.ColorTranslator]::FromHtml('#A7C8C8')
$transparent = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)

# Compact 11x12 opaque bounds inside a 12x12 design grid. The crescent opens
# toward screen-right. S is the only detached spark, separated by one clear pixel.
$pixelMap = @(
    '...OOOO.....',
    '..OTTTTO....',
    '.OTTTTTOOO..',
    'OTTTTOOTTO..',
    'OTTO.OMMO...',
    'OTT..OMMO.S.',
    'ODD..OMMO...',
    'ODDO.OMMO...',
    'ODDDDOODDO..',
    '.ODDDDDOOO..',
    '..ODDDDO....',
    '...OOOO.....'
)

$originX = 45
$originY = 44
$width = 12
$height = 12

$bitmap = New-Object System.Drawing.Bitmap 100, 100, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    for ($y = 0; $y -lt 100; $y++) {
        for ($x = 0; $x -lt 100; $x++) { $bitmap.SetPixel($x, $y, $transparent) }
    }

    for ($y = 0; $y -lt $height; $y++) {
        for ($x = 0; $x -lt $width; $x++) {
            $token = $pixelMap[$y][$x]
            $color = switch ($token) {
                'O' { $outline; break }
                'D' { $darkTeal; break }
                'T' { $turquoise; break }
                'M' { $mint; break }
                'S' { $turquoise; break }
                default { $null }
            }
            if ($null -eq $color) { continue }
            $bitmap.SetPixel($originX + $x, $originY + $y, [System.Drawing.Color]::FromArgb(255, $color.R, $color.G, $color.B))
        }
    }

    $outputDirectory = Split-Path -Parent $Output
    if ($outputDirectory) { [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null }
    $bitmap.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

    $previewBitmap = New-Object System.Drawing.Bitmap 800, 800, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($previewBitmap)
        try {
            $graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#B8B8B8'))
            $checkerBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#969696'))
            try {
                for ($checkerY = 0; $checkerY -lt 800; $checkerY += 80) {
                    for ($checkerX = 0; $checkerX -lt 800; $checkerX += 80) {
                        if ((($checkerX / 80) + ($checkerY / 80)) % 2 -eq 1) {
                            $graphics.FillRectangle($checkerBrush, $checkerX, $checkerY, 80, 80)
                        }
                    }
                }
            }
            finally { $checkerBrush.Dispose() }
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
            $graphics.DrawImage($bitmap, 0, 0, 800, 800)
        }
        finally { $graphics.Dispose() }
        $previewDirectory = Split-Path -Parent $Preview
        if ($previewDirectory) { [System.IO.Directory]::CreateDirectory($previewDirectory) | Out-Null }
        $previewBitmap.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $previewBitmap.Dispose() }
}
finally { $bitmap.Dispose() }
