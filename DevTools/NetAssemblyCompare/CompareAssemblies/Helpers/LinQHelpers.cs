using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLerator.Helpers
{
    public class SequenceItem<T, U>
    {
        public U Sequence { get; set; }
        public T Item { get; set; }
    }
    public static class LinQHelpers
    {
        public static IEnumerable<SequenceItem<T, U>> AddSequence<T, U>(
            this IEnumerable<T> source,
            Func<U, U> incr)
        {
            var sequenceNumber = default(U);


            foreach (var item in source)
            {
                yield return new SequenceItem<T, U>
                {
                    Sequence = sequenceNumber,
                    Item = item
                };
                sequenceNumber = incr(sequenceNumber);
            }
        }
        public static IEnumerable<SequenceItem<T, int>> AddSequence<T>(
            this IEnumerable<T> source)
        {
            return source.AddSequence<T, int>(sequenceNumber => sequenceNumber + 1);
        }


        public static IEnumerable<V> ZIP<T, U, V>(
            this IEnumerable<T> first,
            IEnumerable<U> second,
            Func<T, U, V> ctor)
        {
            var firstIter = first.GetEnumerator();
            var secondIter = second.GetEnumerator();
            firstIter.Reset();
            secondIter.Reset();
            using (firstIter)
            {
                using (secondIter)
                {
                    while (true)
                    {
                        var firstMovedNext = firstIter.MoveNext();
                        var secondMovedNext = secondIter.MoveNext();
                        var movedNext = firstMovedNext && secondMovedNext;

                        if (movedNext.IsFalse())
                        {
                            break;
                        }

                        yield return ctor(firstIter.Current, secondIter.Current);

                    }
                }
            }
        }
    }
}
