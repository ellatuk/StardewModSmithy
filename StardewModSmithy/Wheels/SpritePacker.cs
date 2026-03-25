using System.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModSmithy.Models;
using StardewValley;
using StardewValley.Extensions;

namespace StardewModSmithy.Wheels;

public record TxToPack(string RelPath, Texture2D Texture)
{
    public Rectangle Bounds = new(
        CeilingByTile(Texture.Bounds.X),
        CeilingByTile(Texture.Bounds.Y),
        CeilingByTile(Texture.Bounds.Width),
        CeilingByTile(Texture.Bounds.Height)
    );

    private static int CeilingByTile(int value) => (int)(MathF.Ceiling(value / (float)Utils.TX_TILE) * Utils.TX_TILE);

    public Point TargetPos { get; set; }

    public TxAtlasEntry GetTxAtlasEntry()
    {
        return new TxAtlasEntry(RelPath, new(TargetPos.X, TargetPos.Y, Bounds.Width, Bounds.Height));
    }
}

internal static class SpritePacker
{
    internal static void GatherTexturesToPack(
        DirectoryInfo topDir,
        DirectoryInfo directoryInfo,
        ref List<TxToPack> txToPackList
    )
    {
        foreach (FileInfo info in directoryInfo.EnumerateFiles())
        {
            if (!info.Extension.EqualsIgnoreCase(".png"))
                continue;
            txToPackList.Add(
                new(
                    Path.GetRelativePath(topDir.FullName, info.FullName),
                    ModEntry.ModContent.Load<Texture2D>(Path.GetRelativePath(ModEntry.DirectoryPath, info.FullName))
                )
            );
        }

        foreach (DirectoryInfo info in directoryInfo.EnumerateDirectories())
        {
            GatherTexturesToPack(topDir, info, ref txToPackList);
        }
    }

    internal static void Pack(string subdir, int maxPackedWidth = 512)
    {
        string fullSubdir = Path.Combine(ModEntry.InputDirectoryPath, subdir);
        List<TxToPack> txToPackList = [];
        DirectoryInfo subdirTop = new(fullSubdir);

        GatherTexturesToPack(subdirTop, subdirTop, ref txToPackList);
        if (txToPackList.Count == 0)
        {
            return;
        }

        txToPackList.Sort(
            (a, b) =>
            {
                int hCmp = b.Bounds.Height.CompareTo(a.Bounds.Height);
                if (hCmp != 0)
                    return hCmp;
                return b.Bounds.Width.CompareTo(a.Bounds.Width);
            }
        );

        List<Rectangle> packRects = [new Rectangle(0, 0, maxPackedWidth, int.MaxValue)];
        foreach (TxToPack txToPack in txToPackList)
        {
            Rectangle pickedRect = packRects.FirstOrDefault(rect =>
                rect.Width >= txToPack.Bounds.Width && rect.Height >= txToPack.Bounds.Height
            );
            if (pickedRect.IsEmpty)
            {
                ModEntry.Log($"Failed to pack {txToPack.RelPath} {txToPack.Bounds}", LogLevel.Error);
                break;
            }
            txToPack.TargetPos = new(pickedRect.X, pickedRect.Y);
            packRects.Remove(pickedRect);
            Rectangle hRect = new(
                pickedRect.X + txToPack.Bounds.Width,
                pickedRect.Y,
                pickedRect.Width - txToPack.Bounds.Width,
                txToPack.Bounds.Height
            );
            Rectangle vRect = new(
                pickedRect.X,
                pickedRect.Y + txToPack.Bounds.Height,
                pickedRect.Width,
                pickedRect.Height - txToPack.Bounds.Height
            );

            if (hRect.Width > 0)
                packRects.Insert(0, hRect);
            if (vRect.Height > 0)
                packRects.Add(vRect);
        }
        int maxHeight = packRects.Max(rect => rect.Y);

        Color[] packedData = ArrayPool<Color>.Shared.Rent(maxPackedWidth * maxHeight);
        List<TxAtlasEntry> txAtlasEntries = [];
        Array.Fill(packedData, Color.Transparent);
        foreach (TxToPack txToPack in txToPackList)
        {
            Color[] txToPackData = ArrayPool<Color>.Shared.Rent(txToPack.Texture.GetElementCount());
            Array.Fill(txToPackData, Color.Transparent);
            txToPack.Texture.GetData(txToPackData, 0, txToPack.Texture.GetElementCount());
            CopySourceSpriteToTarget(
                ref txToPackData,
                txToPack.Texture.Width,
                txToPack.Texture.Bounds,
                ref packedData,
                maxPackedWidth,
                new Rectangle(
                    txToPack.TargetPos.X,
                    txToPack.TargetPos.Y,
                    txToPack.Texture.Bounds.Width,
                    txToPack.Texture.Bounds.Height
                )
            );
            ArrayPool<Color>.Shared.Return(txToPackData);
            txAtlasEntries.Add(txToPack.GetTxAtlasEntry());
        }
        using Texture2D packedTx = UnPremultiplyTransparency(maxPackedWidth, maxHeight, packedData);
        ArrayPool<Color>.Shared.Return(packedData);
        using Stream stream = File.Create(Path.Combine(ModEntry.InputDirectoryPath, string.Concat(subdir, ".png")));
        packedTx.SaveAsPng(stream, packedTx.Width, packedTx.Height);

        txAtlasEntries.Sort(
            (a, b) =>
            {
                int cmp1 = a.Area.Y.CompareTo(b.Area.Y);
                if (cmp1 != 0)
                    return cmp1;
                return a.Area.X.CompareTo(b.Area.X);
            }
        );
        ModEntry.WriteJson(ModEntry.InputDirectoryPath, string.Concat(subdir, Utils.ATLAS_SUFFIX), txAtlasEntries);

        ModEntry.Log($"Packed textures from '{subdir}'", LogLevel.Info);
        return;
    }

    /// <summary>Ensure texture width and height is multiple of grid size</summary>
    internal static bool Normalize(string relFile, int gridSize = 16)
    {
        try
        {
            Texture2D texture = ModEntry.ModContent.Load<Texture2D>(relFile);
            int extraW = texture.Width % gridSize;
            int extraH = texture.Height % gridSize;
            if (extraW > 0 || extraH > 0)
            {
                int expandedWidth = texture.Width + extraW;
                int expandedHeight = texture.Height + extraH;
                Color[] originalData = ArrayPool<Color>.Shared.Rent(texture.GetElementCount());
                texture.GetData(originalData, 0, texture.GetElementCount());
                Color[] expandedData = ArrayPool<Color>.Shared.Rent(expandedWidth * expandedHeight);
                Array.Fill(expandedData, Color.Transparent);
                CopySourceSpriteToTarget(
                    ref originalData,
                    texture.Width,
                    texture.Bounds,
                    ref expandedData,
                    expandedWidth,
                    texture.Bounds
                );

                using Texture2D normalizedTexture = UnPremultiplyTransparency(
                    expandedWidth,
                    expandedHeight,
                    expandedData
                );
                using Stream stream = File.Create(Path.Combine(ModEntry.DirectoryPath, relFile));
                normalizedTexture.SaveAsPng(stream, normalizedTexture.Width, normalizedTexture.Height);
                ModEntry.Log(
                    $"Normalized texture '{relFile}' from {texture.Width}x{texture.Height} to {expandedWidth}x{expandedHeight}",
                    LogLevel.Info
                );
            }
            return true;
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Failed to read texture '{relFile}':\n{ex}", LogLevel.Error);
            return false;
        }
    }

    internal static void CopySourceSpriteToTarget(
        ref Color[] sourceData,
        int sourceTxWidth,
        Rectangle sourceRect,
        ref Color[] targetData,
        int targetTxWidth,
        Rectangle targetRect
    )
    {
        for (int r = 0; r < sourceRect.Height; r++)
        {
            int sourceArrayStart = sourceRect.X + (sourceRect.Y + r) * sourceTxWidth;
            int targetArrayStart = targetRect.X + (targetRect.Y + r) * targetTxWidth;
            if (sourceArrayStart + sourceRect.Width > sourceData.Length)
            {
                Array.Fill(targetData, Color.Transparent, targetArrayStart, sourceRect.Width);
            }
            else
            {
                Array.Copy(sourceData, sourceArrayStart, targetData, targetArrayStart, sourceRect.Width);
            }
        }
    }

    /// <summary>Reverse premultiplication applied to an image asset by the XNA content pipeline.</summary>
    /// <param name="texture">The texture to adjust.</param>
    private static Texture2D UnPremultiplyTransparency(int width, int height, Color[] colorData)
    {
        int elementCount = width * height;
        for (int i = 0; i < elementCount; i++)
        {
            Color pixel = colorData[i];
            if (pixel.A == 0)
                continue;

            colorData[i] = new Color(
                (byte)(pixel.R * 255 / pixel.A),
                (byte)(pixel.G * 255 / pixel.A),
                (byte)(pixel.B * 255 / pixel.A),
                pixel.A
            ); // don't use named parameters, which are inconsistent between MonoGame (e.g. 'alpha') and XNA (e.g. 'a')
        }

        Texture2D result = new(Game1.graphics.GraphicsDevice ?? Game1.graphics.GraphicsDevice, width, height);
        result.SetData(colorData, 0, elementCount);
        return result;
    }
}
