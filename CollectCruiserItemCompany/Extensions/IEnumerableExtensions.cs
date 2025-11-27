#nullable enable

using System;
using System.Collections.Generic;

namespace CollectCruiserItemCompany.Extensions;

internal static class IEnumerableExtensions
{
    public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> source, int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0.");
        }

        using var e = source.GetEnumerator();
        while (e.MoveNext())
        {
            var chunk = new List<T>(size) { e.Current };

            while (chunk.Count < size && e.MoveNext())
                chunk.Add(e.Current);

            yield return chunk;
        }
    }
}
