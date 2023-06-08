namespace System {
    using System.Collections;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;

	/// <summary>
	/// <see cref="System.IEquatable{T}"/> extension methods
	/// </summary>
	public static class EnumerableExtensions {

#if NET6_0_OR_GREATER
#else
        // .Chunk<> polyfill available from .net 6 and higher

        /// <summary>
        /// Split the elements of a sequence into chunks of size at most <paramref name="size"/>.
        /// </summary>
        /// <remarks>
        /// Every chunk except the last will be of size <paramref name="size"/>.
        /// The last chunk will contain the remaining elements and may be of a smaller size.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> whose elements to chunk.
        /// </param>
        /// <param name="size">
        /// Maximum size of each chunk.
        /// </param>
        /// <typeparam name="TSource">
        /// The type of the elements of source.
        /// </typeparam>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains the elements the input sequence split into chunks of size <paramref name="size"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size"/> is below 1.
        /// </exception>
        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size) {
            if(source is null)
                throw new ArgumentNullException(nameof(source));

            if(size < 1)
                throw new ArgumentOutOfRangeException(nameof(size), "must be >= 1");

            return ChunkIterator(source, size);
        }

        private static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size) {
            using IEnumerator<TSource> e = source.GetEnumerator();

            // Before allocating anything, make sure there's at least one element.
            if(e.MoveNext()) {
                // Now that we know we have at least one item, allocate an initial storage array. This is not
                // the array we'll yield.  It starts out small in order to avoid significantly overallocating
                // when the source has many fewer elements than the chunk size.
                int arraySize = Math.Min(size, 4);
                int i;
                do {
                    var array = new TSource[arraySize];

                    // Store the first item.
                    array[0] = e.Current;
                    i = 1;

                    if(size != array.Length) {
                        // This is the first chunk. As we fill the array, grow it as needed.
                        for(; i < size && e.MoveNext(); i++) {
                            if(i >= array.Length) {
                                arraySize = (int)Math.Min((uint)size, 2 * (uint)array.Length);
                                Array.Resize(ref array, arraySize);
                            }

                            array[i] = e.Current;
                        }
                    } else {
                        // For all but the first chunk, the array will already be correctly sized.
                        // We can just store into it until either it's full or MoveNext returns false.
                        TSource[] local = array; // avoid bounds checks by using cached local (`array` is lifted to iterator object as a field)
                        Debug.Assert(local.Length == size);
                        for(; (uint)i < (uint)local.Length && e.MoveNext(); i++) {
                            local[i] = e.Current;
                        }
                    }

                    if(i != array.Length) {
                        Array.Resize(ref array, i);
                    }

                    yield return array;
                }
                while(i >= size && e.MoveNext());
            }
        }

        // TryGetNonEnumeratedCount polyfill from .NET 6 and higher

        /// <summary>
        ///   Attempts to determine the number of elements in a sequence without forcing an enumeration.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence that contains elements to be counted.</param>
        /// <param name="count">
        ///     When this method returns, contains the count of <paramref name="source" /> if successful,
        ///     or zero if the method failed to determine the count.</param>
        /// <returns>
        ///   <see langword="true" /> if the count of <paramref name="source"/> can be determined without enumeration;
        ///   otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///   The method performs a series of type tests, identifying common subtypes whose
        ///   count can be determined without enumerating; this includes <see cref="ICollection{T}"/>,
        ///   <see cref="ICollection"/> as well as internal types used in the LINQ implementation.
        ///
        ///   The method is typically a constant-time operation, but ultimately this depends on the complexity
        ///   characteristics of the underlying collection implementation.
        /// </remarks>
        public static bool TryGetNonEnumeratedCount<TSource>(this IEnumerable<TSource> source, out int count) {
            if(source is null)
                throw new ArgumentNullException(nameof(source));

            if(source is ICollection<TSource> collectionoft) {
                count = collectionoft.Count;
                return true;
            }

            if(source is ICollection collection) {
                count = collection.Count;
                return true;
            }

            count = 0;
            return false;
        }

#endif

        /// <summary>
        /// Performs a specific action on each element of the sequence
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if(source == null)
                throw new ArgumentNullException(nameof(source));
            if(action == null)
                throw new ArgumentNullException(nameof(action));

            foreach(T element in source) {
                action(element);

                yield return element;
            }
        }

		/// <summary>
		/// Iterates over two <see cref="IEnumerable"/> until one of them reaches the end of elements
		/// </summary>
		/// <typeparam name="TFirst">Types of elements in the first sequence</typeparam>
		/// <typeparam name="TSecond">Types of elements in the second sequence</typeparam>
		/// <param name="first">First sequence</param>
		/// <param name="second">Second sequence</param>
		/// <returns>Sequence of tuples from the first and second sequences</returns>
		public static IEnumerable<Tuple<TFirst, TSecond>> MultiIterate<TFirst, TSecond>(
		   IEnumerable<TFirst> first, IEnumerable<TSecond> second) {
			if (first == null || second == null) yield break;

			IEnumerator<TFirst> firstEnumerator = first.GetEnumerator();
			IEnumerator<TSecond> secondEnumerator = second.GetEnumerator();

			while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext()) {
				yield return Tuple.Create(firstEnumerator.Current, secondEnumerator.Current);
			}

			yield break;
		}
	}
}