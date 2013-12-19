/* Copyright 2013-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public static class EnumerableAsync
    {
        // static methods
        public static IEnumerable<TSource> AsEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (enumerator.MoveNextAsync().GetAwaiter().GetResult())
                {
                    yield return enumerator.Current;
                }
            }
        }

        public static Task<TSource> ElementAtAsync<TSource>(this IAsyncEnumerable<TSource> source, int index)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (index < 0) { throw new ArgumentOutOfRangeException("index"); }
            return ElementAtAsyncInternal(source, index);
        }

        private static async Task<TSource> ElementAtAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, int index)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    if (index == 0)
                    {
                        return enumerator.Current;
                    }
                    index--;
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public static Task<TSource> ElementAtOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, int index)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (index < 0) { throw new ArgumentOutOfRangeException("index"); }
            return ElementAtOrDefaultAsyncInternal(source, index);
        }

        private static async Task<TSource> ElementAtOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, int index)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    if (index == 0)
                    {
                        return enumerator.Current;
                    }
                    index--;
                }
            }
            return default(TSource);
        }

        public static IAsyncEnumerable<TResult> Empty<TResult>()
        {
            return new EmptyAsyncEnumerable<TResult>();
        }

        public static async Task ForEachAsync<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> body)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    body(enumerator.Current);
                }
            }
        }

        public static async Task ForEachAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> body)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    await body(enumerator.Current);
                }
            }
        }

        public static Task<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return FirstAsyncInternal(source);
        }

        private static async Task<TSource> FirstAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync())
                {
                    return enumerator.Current;
                }
            }
            throw new InvalidOperationException("No elements.");
        }

        public static Task<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (predicate == null) { throw new ArgumentNullException("predicate"); }
            return FirstAsyncInternal(source, predicate);
        }

        private static async Task<TSource> FirstAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    var element = enumerator.Current;
                    if (predicate(element))
                    {
                        return element;
                    }
                }
            }
            throw new InvalidOperationException("No matching elements.");
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return FirstOrDefaultAsyncInternal(source);
        }

        private static async Task<TSource> FirstOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync())
                {
                    return enumerator.Current;
                }
            }
            return default(TSource);
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (predicate == null) { throw new ArgumentNullException("predicate"); }
            return FirstOrDefaultAsyncInternal(source, predicate);
        }

        private static async Task<TSource> FirstOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    var element = enumerator.Current;
                    if (predicate(element))
                    {
                        return element;
                    }
                }
            }
            return default(TSource);
        }

        public static Task<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return LastAsyncInternal(source);
        }

        private static async Task<TSource> LastAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync())
                {
                    TSource last;
                    do
                    {
                        last = enumerator.Current;
                    }
                    while (await enumerator.MoveNextAsync());
                    return last;
                }
            }
            throw new InvalidOperationException("No elements.");
        }

        public static Task<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (predicate == null) { throw new ArgumentNullException("predicate"); }
            return LastAsyncInternal(source, predicate);
        }

        private static async Task<TSource> LastAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var last = default(TSource);
            var foundMatch = false;
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    var element = enumerator.Current;
                    if (predicate(element))
                    {
                        last = element;
                        foundMatch = true;
                    }
                }
            }
            if (!foundMatch)
            {
                throw new InvalidOperationException("No matching elements.");
            }
            return last;
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return LastOrDefaultAsyncInternal(source);
        }

        private static async Task<TSource> LastOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync())
                {
                    TSource last;
                    do
                    {
                        last = enumerator.Current;
                    }
                    while (await enumerator.MoveNextAsync());
                    return last;
                }
            }
            return default(TSource);
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (predicate == null) { throw new ArgumentNullException("predicate"); }
            return LastOrDefaultAsyncInternal(source, predicate);
        }

        private static async Task<TSource> LastOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var lastOrDefault = default(TSource);
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    var element = enumerator.Current;
                    if (predicate(element))
                    {
                        lastOrDefault = element;
                    }
                }
            }
            return lastOrDefault;
        }

        public static Task<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return SingleAsyncInternal(source);
        }

        private static async Task<TSource> SingleAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync())
                {
                    var single = enumerator.Current;
                    if (await enumerator.MoveNextAsync())
                    {
                        throw new InvalidOperationException("More than one element.");
                    }
                    return single;
                }
            }
            throw new InvalidOperationException("No elements.");
        }

        public static Task<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (predicate == null) { throw new ArgumentNullException("predicate"); }
            return SingleAsyncInternal(source, predicate);
        }

        private static async Task<TSource> SingleAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var single = default(TSource);
            var foundMatch = false;
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    var element = enumerator.Current;
                    if (predicate(element))
                    {
                        if (foundMatch)
                        {
                            throw new InvalidOperationException("More than one matching element.");
                        }
                        else
                        {
                            single = element;
                            foundMatch = true;
                        }
                    }
                }
            }
            if (foundMatch)
            {
                return single;
            }
            throw new InvalidOperationException("No matching elements.");
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return SingleOrDefaultAsyncInternal(source);
        }

        private static async Task<TSource> SingleOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync())
                {
                    var single = enumerator.Current;
                    if (await enumerator.MoveNextAsync())
                    {
                        throw new InvalidOperationException("More than one element.");
                    }
                    return single;
                }
            }
            return default(TSource);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (predicate == null) { throw new ArgumentNullException("predicate"); }
            return SingleOrDefaultAsyncInternal(source, predicate);
        }

        private static async Task<TSource> SingleOrDefaultAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var singleOrDefault = default(TSource);
            var foundMatch = false;
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    var element = enumerator.Current;
                    if (predicate(element))
                    {
                        if (foundMatch)
                        {
                            throw new InvalidOperationException("More than one matching element.");
                        }
                        else
                        {
                            singleOrDefault = element;
                            foundMatch = true;
                        }
                    }
                }
            }
            return singleOrDefault;
        }

        public static IAsyncEnumerable<TSource> Skip<TSource>(this IAsyncEnumerable<TSource> source, int count)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return new SkipAsyncEnumerable<TSource>(source, count);
        }

        public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return new SkipWhileAsyncEnumerable<TSource>(source, predicate);
        }

        public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return new SkipWhileAsyncEnumerable<TSource>(source, predicate);
        }

        public static IAsyncEnumerable<TSource> Take<TSource>(this IAsyncEnumerable<TSource> source, int count)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return new TakeAsyncEnumerable<TSource>(source, count);
        }

        public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return new TakeWhileAsyncEnumerable<TSource>(source, predicate);
        }

        public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return new TakeWhileAsyncEnumerable<TSource>(source, predicate);
        }

        public static Task<TSource[]> ToArrayAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return ToArrayAsyncInternal(source);
        }

        private static async Task<TSource[]> ToArrayAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            var list = await source.ToListAsync();
            return list.ToArray();
        }

        public static Task<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            return ToListAsyncInternal(source);
        }

        private static async Task<List<TSource>> ToListAsyncInternal<TSource>(this IAsyncEnumerable<TSource> source)
        {
            var list = new List<TSource>();
            using (var enumerator = source.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync())
                {
                    list.Add(enumerator.Current);
                }
            }
            return list;
        }
    }
}
