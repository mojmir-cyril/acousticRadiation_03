#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member



using System;
using System.Collections.Generic;
using System.Linq;

//
//  Ansys:
//


namespace SVSEntityManagerF472
{
    public static class SExtensions
    {
        /// <summary>
        /// list.Enumerate()
        /// </summary>
        /// <example>
        /// <code>
        /// foreach ((int i, SEntity ent) in ents.Enumerate())
        /// {
        ///   i   ---> 0,        1,        2,        3, ...
        ///   ent ---> ents[0],  ents[1],  ents[2],  ents[3], ...
        /// }
        /// </code>
        /// </example>
        /// <typeparam name="TGeneralized"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<(int, TGeneralized)> Enumerate<TGeneralized>(this IEnumerable<TGeneralized> array) 
            => array.Select((item, index) => (index, item));
        /// <summary>
        /// keys.Dictionary(key => ValuesFunc(key)) converts list of keys to dictionary via ValuesFunc 
        /// </summary>
        /// <example>
        /// <code>
        /// keys.Dictionary(key => ValuesFunc(key))
        /// keys.Dictionary(ValuesFunc)
        /// </code>
        /// </example>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="keys"></param>
        /// <param name="ValuesFunc"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TKey> keys, Func<TKey, TValue> ValuesFunc) 
            => keys.Zip(keys.Select(id => ValuesFunc(id)), (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v); 
        /// <summary>
        /// keys.Dictionary(values) converts list of keys and values to dictionary 
        /// </summary>
        /// <example>
        /// <code>
        /// keys.Dictionary(values);
        /// keys.Dictionary(keys.Select(key => ValuesFunc(key)));
        /// </code>
        /// </example>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values) 
            => keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        /// <summary>
        /// list1.ChunkBy(chunkSize: 3) ---> [1,2,3,4,5,7,8,9] => [[1,2,3],[4,5,6],[7,8,9]]
        /// </summary>
        /// <example>
        /// <code>
        /// List[int] values = new List[int] { 1,2,3,4,5,7,8,9 };
        /// values.ChunkBy(3);
        /// </code>
        /// </example>
        public static List<List<T>> ChunkBy<T>(this IList<T> source, int chunkSize)
        {
            return source.Select((x, i) => new { Index = i, Value = x })
                         .GroupBy(x => x.Index / chunkSize)
                         .Select(x => x.Select(v => v.Value).ToList())
                         .ToList();
        }
        /// <summary>
        /// array.ChunkBy(chunkSize: 3) ---> [1,2,3,4,5,7,8,9] => [[1,2,3],[4,5,6],[7,8,9]]
        /// <example>
        /// <code>
        /// int[] values = new int[] { 1,2,3,4,5,7,8,9 };
        /// values.ChunkBy(3);
        /// </code>
        /// </example>
        /// </summary>
        public static List<T[]> ChunkBy<T>(this T[] source, int chunkSize)  
        {
            return source.Select((x, i) => new { Index = i, Value = x })
                         .GroupBy(x => x.Index / chunkSize)
                         .Select(x => x.Select(v => v.Value).ToArray())
                         .ToList();
        }
    }
}
 
