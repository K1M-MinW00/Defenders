param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$palette = @(
    [System.Drawing.ColorTranslator]::FromHtml('#1F1A1A'),
    [System.Drawing.ColorTranslator]::FromHtml('#302D36'),
    [System.Drawing.ColorTranslator]::FromHtml('#45424C'),
    [System.Drawing.ColorTranslator]::FromHtml('#5D5963'),
    [System.Drawing.ColorTranslator]::FromHtml('#303958'),
    [System.Drawing.ColorTranslator]::FromHtml('#414E72'),
    [System.Drawing.ColorTranslator]::FromHtml('#556482'),
    [System.Drawing.ColorTranslator]::FromHtml('#667895'),
    [System.Drawing.ColorTranslator]::FromHtml('#425C6B'),
    [System.Drawing.ColorTranslator]::FromHtml('#607581'),
    [System.Drawing.ColorTranslator]::FromHtml('#7F909A'),
    [System.Drawing.ColorTranslator]::FromHtml('#9BADB7'),
    [System.Drawing.ColorTranslator]::FromHtml('#4C3029'),
    [System.Drawing.ColorTranslator]::FromHtml('#694133'),
    [System.Drawing.ColorTranslator]::FromHtml('#865847'),
    [System.Drawing.ColorTranslator]::FromHtml('#8F887A'),
    [System.Drawing.ColorTranslator]::FromHtml('#B7A88E'),
    [System.Drawing.ColorTranslator]::FromHtml('#D0C7B5'),
    [System.Drawing.ColorTranslator]::FromHtml('#88A9AD'),
    [System.Drawing.ColorTranslator]::FromHtml('#A7C8C8')
)

function Get-NearestPaletteColor([System.Drawing.Color]$Color) {
    $best = $palette[0]
    $bestDistance = [double]::MaxValue
    foreach ($candidate in $palette) {
        $dr = [int]$Color.R - [int]$candidate.R
        $dg = [int]$Color.G - [int]$candidate.G
        $db = [int]$Color.B - [int]$candidate.B
        $distance = 0.30 * $dr * $dr + 0.59 * $dg * $dg + 0.11 * $db * $db
        if ($distance -lt $bestDistance) { $bestDistance = $distance; $best = $candidate }
    }
    return $best
}

$input = [System.Drawing.Bitmap]::FromFile($Source)
try {
    $minX = $input.Width; $minY = $input.Height; $maxX = -1; $maxY = -1
    for ($y = 0; $y -lt $input.Height; $y++) {
        for ($x = 0; $x -lt $input.Width; $x++) {
            if ($input.GetPixel($x, $y).A -lt 24) { continue }
            $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
            $minY = [Math]::Min($minY, $y); $maxY = [Math]::Max($maxY, $y)
        }
    }
    if ($maxX -lt 0) { throw 'Source has no opaque pixels.' }

    $sourceWidth = $maxX - $minX + 1
    $sourceHeight = $maxY - $minY + 1
    $scale = [Math]::Min(25.0 / $sourceWidth, 29.0 / $sourceHeight)
    # A minimum 24px width keeps the face, hood, and sleeve separated at gameplay scale.
    # The generated source tends to be too vertically narrow because of the staff.
    $targetWidth = [Math]::Min(25, [Math]::Max(24, [int][Math]::Round($sourceWidth * $scale)))
    $targetHeight = [Math]::Max(1, [int][Math]::Round($sourceHeight * $scale))
    $targetX = [int][Math]::Floor((100 - $targetWidth) / 2)
    $targetY = 56 - $targetHeight

    $result = New-Object System.Drawing.Bitmap 100, 100, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($result)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $srcRect = New-Object System.Drawing.Rectangle $minX, $minY, $sourceWidth, $sourceHeight
            $dstRect = New-Object System.Drawing.Rectangle $targetX, $targetY, $targetWidth, $targetHeight
            $graphics.DrawImage($input, $dstRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
        }
        finally { $graphics.Dispose() }

        for ($y = 0; $y -lt 100; $y++) {
            for ($x = 0; $x -lt 100; $x++) {
                $color = $result.GetPixel($x, $y)
                if ($color.A -lt 112) { $result.SetPixel($x, $y, [System.Drawing.Color]::Transparent); continue }
                $mapped = Get-NearestPaletteColor $color
                $result.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
            }
        }

        $result.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

        $large = New-Object System.Drawing.Bitmap 800, 800, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $g = [System.Drawing.Graphics]::FromImage($large)
            try {
                $g.Clear([System.Drawing.Color]::FromArgb(145, 72, 55))
                $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $g.DrawImage($result, 0, 0, 800, 800)
            }
            finally { $g.Dispose() }
            $large.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $large.Dispose() }

        "output=${targetWidth}x${targetHeight} bounds=($targetX,$targetY) palette=$($palette.Count)"
    }
    finally { $result.Dispose() }
}
finally { $input.Dispose() }
