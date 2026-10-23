<#
.SYNOPSIS
    Split one character's GIF actions into PNG frames on a shared canvas.

.DESCRIPTION
    Kept ASCII-only: Windows PowerShell 5.1 reads BOM-less UTF-8 scripts with the
    system code page, and .editorconfig requires BOM-less UTF-8.

    1. Downscale upscaled pixel art by -Scale (every Scale x Scale block must be one
       color, otherwise the script stops).
    2. Align all actions on the feet center: the midpoint of the left and right edges
       of the lowest opaque row.
    3. Crop to the union of opaque pixels of every frame, with the feet center at the
       bottom middle of the canvas.
    4. Write <OutRoot>/<Action>/<Name>_<Action>_<two-digit index>.png and print the
       GIF delay of each frame.

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File Tools/split_gif.ps1 `
        -Name Player -Scale 3 -OutRoot Assets/_Project/Art/Sprites/Characters/Player `
        -Inputs "Idle=ArtSource/Characters/Player/player_idle.gif,Attack=ArtSource/Characters/Player/player_attack.gif"
#>
param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string[]]$Inputs,
    [Parameter(Mandatory = $true)][string]$OutRoot,
    [int]$Scale = 1
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Read-GifFrames([string]$path, [int]$scale)
{
    $image = [System.Drawing.Image]::FromFile((Resolve-Path $path).Path)
    try
    {
        $dimension = New-Object System.Drawing.Imaging.FrameDimension($image.FrameDimensionsList[0])
        $count = $image.GetFrameCount($dimension)
        $delayBytes = $image.GetPropertyItem(0x5100).Value
        $frames = @()
        for ($i = 0; $i -lt $count; $i++)
        {
            $image.SelectActiveFrame($dimension, $i) | Out-Null
            $source = New-Object System.Drawing.Bitmap($image)
            if ($source.Width % $scale -ne 0 -or $source.Height % $scale -ne 0)
            {
                throw "$path size $($source.Width)x$($source.Height) is not a multiple of $scale"
            }

            $w = $source.Width / $scale
            $h = $source.Height / $scale
            $bitmap = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            for ($y = 0; $y -lt $h; $y++)
            {
                for ($x = 0; $x -lt $w; $x++)
                {
                    $color = $source.GetPixel($x * $scale, $y * $scale)
                    for ($dy = 0; $dy -lt $scale; $dy++)
                    {
                        for ($dx = 0; $dx -lt $scale; $dx++)
                        {
                            if ($source.GetPixel($x * $scale + $dx, $y * $scale + $dy).ToArgb() -ne $color.ToArgb())
                            {
                                throw "$path frame $i block ($x, $y) is not one color; -Scale may be wrong"
                            }
                        }
                    }

                    if ($color.A -eq 0)
                    {
                        $color = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
                    }

                    $bitmap.SetPixel($x, $y, $color)
                }
            }

            $source.Dispose()
            $frames += [pscustomobject]@{
                Bitmap = $bitmap
                DelayMs = [BitConverter]::ToInt32($delayBytes, $i * 4) * 10
            }
        }

        return , $frames
    }
    finally
    {
        $image.Dispose()
    }
}

# Right and Bottom are pixel edges (exclusive).
function Get-OpaqueBounds($bitmap)
{
    $minX = [int]::MaxValue; $minY = [int]::MaxValue; $maxX = -1; $maxY = -1
    for ($y = 0; $y -lt $bitmap.Height; $y++)
    {
        for ($x = 0; $x -lt $bitmap.Width; $x++)
        {
            if ($bitmap.GetPixel($x, $y).A -gt 0)
            {
                if ($x -lt $minX) { $minX = $x }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($y -gt $maxY) { $maxY = $y }
            }
        }
    }

    return [pscustomobject]@{ Left = $minX; Top = $minY; Right = $maxX + 1; Bottom = $maxY + 1 }
}

function Get-FeetCenter($bitmap, [int]$bottom)
{
    $row = $bottom - 1
    $minX = [int]::MaxValue; $maxX = -1
    for ($x = 0; $x -lt $bitmap.Width; $x++)
    {
        if ($bitmap.GetPixel($x, $row).A -gt 0)
        {
            if ($x -lt $minX) { $minX = $x }
            if ($x -gt $maxX) { $maxX = $x }
        }
    }

    return ($minX + $maxX + 1) / 2.0
}

# With -File, an array argument arrives as one comma-separated string.
$Inputs = $Inputs | ForEach-Object { $_.Split(',') } | Where-Object { $_ }

$actions = @()
foreach ($entry in $Inputs)
{
    $parts = $entry.Split('=', 2)
    if ($parts.Count -ne 2)
    {
        throw "Inputs entries must be <Action>=<gif path>: $entry"
    }

    $frames = Read-GifFrames $parts[1] $Scale

    $union = $null
    foreach ($frame in $frames)
    {
        $b = Get-OpaqueBounds $frame.Bitmap
        if ($null -eq $union) { $union = $b; continue }
        $union.Left = [Math]::Min($union.Left, $b.Left)
        $union.Top = [Math]::Min($union.Top, $b.Top)
        $union.Right = [Math]::Max($union.Right, $b.Right)
        $union.Bottom = [Math]::Max($union.Bottom, $b.Bottom)
    }

    # The character stays in place within one action, so the first frame gives the feet.
    $first = Get-OpaqueBounds $frames[0].Bitmap
    $feet = Get-FeetCenter $frames[0].Bitmap $first.Bottom
    if ($feet -ne [Math]::Floor($feet))
    {
        throw "$($parts[0]) feet center x = $feet is not on a pixel edge; it cannot be centered on an even-width canvas"
    }

    $actions += [pscustomobject]@{
        Action = $parts[0]
        Frames = $frames
        Bounds = $union
        FeetX = [int]$feet
        FeetY = $first.Bottom
    }
}

# Shared canvas measured from the feet: widest half-width, tallest height.
$halfWidth = 0
$height = 0
foreach ($a in $actions)
{
    $halfWidth = [Math]::Max($halfWidth, [Math]::Max($a.FeetX - $a.Bounds.Left, $a.Bounds.Right - $a.FeetX))
    $height = [Math]::Max($height, $a.FeetY - $a.Bounds.Top)
    if ($a.Bounds.Bottom -gt $a.FeetY)
    {
        Write-Warning "$($a.Action) has pixels below the feet; they will be cropped"
    }
}

$width = $halfWidth * 2
"Canvas: ${width}x$height"

foreach ($a in $actions)
{
    $dir = Join-Path $OutRoot $a.Action
    New-Item -ItemType Directory -Force $dir | Out-Null
    $offsetX = $halfWidth - $a.FeetX
    $offsetY = $height - $a.FeetY
    for ($i = 0; $i -lt $a.Frames.Count; $i++)
    {
        $canvas = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $source = $a.Frames[$i].Bitmap
        for ($y = 0; $y -lt $source.Height; $y++)
        {
            for ($x = 0; $x -lt $source.Width; $x++)
            {
                $tx = $x + $offsetX; $ty = $y + $offsetY
                if ($tx -ge 0 -and $tx -lt $width -and $ty -ge 0 -and $ty -lt $height)
                {
                    $canvas.SetPixel($tx, $ty, $source.GetPixel($x, $y))
                }
            }
        }

        $file = Join-Path $dir ("{0}_{1}_{2:D2}.png" -f $Name, $a.Action, $i)
        $canvas.Save($file, [System.Drawing.Imaging.ImageFormat]::Png)
        $canvas.Dispose()
        $source.Dispose()
    }

    $delays = ($a.Frames | ForEach-Object { $_.DelayMs }) -join ', '
    "{0}: {1} frames, delays (ms) = {2}" -f $a.Action, $a.Frames.Count, $delays
}
