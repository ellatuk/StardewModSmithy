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

    private static int CeilingByTile(int value) => (int)(MathF.Ceiling(value / (float)Consts.TX_TILE) * Consts.TX_TILE);

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
            string relPath = Path.GetRelativePath(ModEntry.DirectoryPath, info.FullName);
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
        string fullSubdir = Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_INPUT, subdir);
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

        Texture2D packedTx = new(Game1.graphics.GraphicsDevice, maxPackedWidth, maxHeight);
        Color[] packedData = ArrayPool<Color>.Shared.Rent(packedTx.GetElementCount());
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
                packedTx.Width,
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
        packedTx.SetData(packedData, 0, packedTx.GetElementCount());
        ArrayPool<Color>.Shared.Return(packedData);

        using Texture2D forExport = UnPremultiplyTransparency(packedTx);
        using Stream stream = File.Create(
            Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_INPUT, string.Concat(subdir, ".png"))
        );
        forExport.SaveAsPng(stream, forExport.Width, forExport.Height);

        txAtlasEntries.Sort(
            (a, b) =>
            {
                int cmp1 = a.Area.Y.CompareTo(b.Area.Y);
                if (cmp1 != 0)
                    return cmp1;
                return a.Area.X.CompareTo(b.Area.X);
            }
        );
        ModEntry.WriteJson(
            Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_INPUT),
            string.Concat(subdir, Consts.ATLAS_SUFFIX),
            txAtlasEntries
        );

        ModEntry.Log($"Packed textures from '{subdir}'");
        return;
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
    private static Texture2D UnPremultiplyTransparency(Texture2D texture)
    {
        Color[] data = new Color[texture.Width * texture.Height];
        texture.GetData(data);

        for (int i = 0; i < data.Length; i++)
        {
            Color pixel = data[i];
            if (pixel.A == 0)
                continue;

            data[i] = new Color(
                (byte)(pixel.R * 255 / pixel.A),
                (byte)(pixel.G * 255 / pixel.A),
                (byte)(pixel.B * 255 / pixel.A),
                pixel.A
            ); // don't use named parameters, which are inconsistent between MonoGame (e.g. 'alpha') and XNA (e.g. 'a')
        }

        Texture2D result = new(texture.GraphicsDevice ?? Game1.graphics.GraphicsDevice, texture.Width, texture.Height);
        result.SetData(data);
        return result;
    }
}
