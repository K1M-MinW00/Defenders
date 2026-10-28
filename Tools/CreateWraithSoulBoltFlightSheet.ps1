param(
    [Parameter(Mandatory = $true)][string]$Reference,
    [Parameter(Mandatory = $true)][string]$Output
)

Add-Type -AssemblyName System.Drawing

$outlineArgb = [System.Drawing.ColorTranslator]::FromHtml('#1F1A1A').ToArgb()
$darkTealArgb = [System.Drawing.ColorTranslator]::FromHtml('#425C6B').ToArgb()
$turquoiseArgb = [System.Drawing.ColorTranslator]::FromHtml('#607581').ToArgb()
$mintArgb = [System.Drawing.ColorTranslator]::FromHtml('#A7C8C8').ToArgb()
$allowedArgb = @($outlineArgb, $darkTealArgb, $turquoiseArgb, $mintArgb)
$transparent = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)

$referenceBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path $Reference))
try {
    if ($referenceBitmap.Width -ne 100 -or $referenceBitmap.Height -ne 100) {
        throw "Reference must be exactly 100x100, found $($referenceBitmap.Width)x$($referenceBitmap.Height)."
    }

    for ($y = 0; $y -lt 100; $y++) {
        for ($x = 0; $x -lt 100; $x++) {
            $pixel = $referenceBitmap.GetPixel($x, $y)
            if ($pixel.A -ne 0 -and $pixel.A -ne 255) { throw "Reference contains non-binary alpha at $x,$y." }
            if ($pixel.A -eq 255 -and $allowedArgb -notcontains $pixel.ToArgb()) {
                throw "Reference contains an unapproved color at $x,$y."
            }
        }
    }

    $sheet = New-Object System.Drawing.Bitmap 600, 100, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($frame = 0; $frame -lt 6; $frame++) {
            for ($y = 0; $y -lt 100; $y++) {
                for ($x = 0; $x -lt 100; $x++) {
                    $sheet.SetPixel($frame * 100 + $x, $y, $referenceBitmap.GetPixel($x, $y))
                }
            }
        }

        function Set-FramePixel([int]$frame, [int]$x, [int]$y, [System.Drawing.Color]$color) {
            $sheet.SetPixel($frame * 100 + $x, $y, $color)
        }

        $mint = [System.Drawing.Color]::FromArgb(255, 167, 200, 200)
        $turquoise = [System.Drawing.Color]::FromArgb(255, 96, 117, 129)

        # Frame 2: brighten the ember with one approved palette-step pixel and
        # contract the crescent's upper edge by exactly one pixel.
        Set-FramePixel 1 52 47 $mint
        Set-FramePixel 1 48 44 $transparent

        # Frame 3: one-pixel inward compression at the crescent's left contour;
        # move the sole detached spark exactly one pixel upward.
        Set-FramePixel 2 45 47 $transparent
        Set-FramePixel 2 45 52 $transparent
        Set-FramePixel 2 55 49 $transparent
        Set-FramePixel 2 55 48 $turquoise

        # Frame 4: base ember, lower edge changes by exactly one pixel.
        Set-FramePixel 3 48 55 $transparent

        # Frame 5: open the forward crescent edge by one pixel and move the sole
        # detached spark exactly one pixel downward.
        Set-FramePixel 4 54 47 $transparent
        Set-FramePixel 4 55 49 $transparent
        Set-FramePixel 4 55 50 $turquoise

        # Frame 6 intentionally remains an exact reference copy for a seamless loop.

        $outputDirectory = Split-Path -Parent $Output
        if ($outputDirectory) { [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null }
        $sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $sheet.Dispose() }
}
finally { $referenceBitmap.Dispose() }
