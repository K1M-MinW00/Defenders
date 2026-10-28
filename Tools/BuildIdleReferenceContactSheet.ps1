param(
    [Parameter(Mandatory = $true)][string[]]$Sources,
    [Parameter(Mandatory = $true)][string]$Output
)

Add-Type -AssemblyName System.Drawing

$scale = 8
$cell = 100
$rowHeight = $cell * $scale
$canvas = New-Object System.Drawing.Bitmap (6 * $cell * $scale), ($Sources.Count * $rowHeight), ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    $graphics = [System.Drawing.Graphics]::FromImage($canvas)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(34, 37, 44))
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        for ($row = 0; $row -lt $Sources.Count; $row++) {
            $source = [System.Drawing.Bitmap]::FromFile($Sources[$row])
            try {
                for ($frame = 0; $frame -lt 6; $frame++) {
                    $src = New-Object System.Drawing.Rectangle ($frame * $cell), 0, $cell, $cell
                    $dst = New-Object System.Drawing.Rectangle ($frame * $cell * $scale), ($row * $rowHeight), ($cell * $scale), ($cell * $scale)
                    $graphics.DrawImage($source, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel)
                }
            }
            finally { $source.Dispose() }
        }
    }
    finally { $graphics.Dispose() }
    $canvas.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $canvas.Dispose() }
