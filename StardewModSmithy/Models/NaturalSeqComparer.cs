namespace StardewModSmithy.Models;

public class NaturalSeqComparer() : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y)
            return 0;
        if (x == null)
            return -1;
        if (y == null)
            return 1;
        string[] x_parts = x.Split('_');
        string[] y_parts = y.Split('_');
        if (x_parts.Length != y_parts.Length)
            return x.CompareTo(y);
        for (int i = 0; i < x_parts.Length - 1; i++)
        {
            if (x_parts[i] != y_parts[i])
                return x.CompareTo(y);
        }
        if (
            x_parts.Length == 0
            || !int.TryParse(x_parts[^1], out int x_seq)
            || y_parts.Length == 0
            || !int.TryParse(y_parts[^1], out int y_seq)
        )
            return x.CompareTo(y);
        return x_seq.CompareTo(y_seq);
    }
}
