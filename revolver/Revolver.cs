using System;
using System.Threading;

namespace Concurrent
{
    /// <summary>
    /// A thread-safe circular buffer in C# is implementing a non-blocking producer-consumer pattern.
    /// The object is disposable which means its ownership needs to be taken care of.
    /// The object ownership will be transferred from producer to consumer.
    /// The consumer is responsible to dispose the object once finished.
    /// Any unconsumed (dropped) objects will be disposed automatically in a thread-safe way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Revolver<T> where T : class, IDisposable
    {
        private readonly T[] _buffer;

        private readonly int _bufferSize;
        private int _head;
        private int _tail;

        public Revolver(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(
                    "Circular buffer capacity must be positive.", nameof(capacity));
            }
            _head = 0;
            _tail = 0;
            _bufferSize = capacity + 1;
            _buffer = new T[_bufferSize];
        }

        /// <summary>
        /// Maximum capacity of the buffer. If the maximum capacity is reached,
        /// then overwrite the next element.
        /// </summary>
        public int Capacity => _bufferSize - 1;

        /// <summary>
        /// The size of buffer
        /// </summary>
        public int Count
        {
            get
            { 
                lock (_buffer) 
                { 
                    return (_head - _tail + _bufferSize) % _bufferSize; 
                } 
            }
        }

        /// <summary>
        /// Signal the consumer to finish the loop.
        /// </summary>
        public void Finish()
        {
            // Enqueue one null item to make it exit.
            AddOne(null);
        }

        /// <summary>
        /// If the buffer is empty.
        /// </summary>
        private bool IsEmpty => _head == _tail;

        /// <summary>
        ///  Increments the index variable by one with wrapping around.
        /// </summary>
        /// <param name="value"></param>
        private void Increment(ref int value) 
        { 
            value = (value + 1) % _bufferSize; 
        }

        /// <summary>
        /// Adds the item to circular buffer
        /// Null item will not be added into queue
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (item != null)
            {
                AddOne(item);
            }
        }
        private void AddOne(T item)
        {
            lock (_buffer)
            {
                _buffer[_head]?.Dispose();
                _buffer[_head] = item;
                Increment(ref _head);
                if (IsEmpty)
                {
                    Increment(ref _tail);
                }
                Monitor.PulseAll(_buffer);
            }
        }

        /// <summary>
        /// Removes an item from the circular buffer
        /// The item ownership is transferred to consumer. The consumer need to dispose the item.
        /// Null item signals exit of the loop.
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            lock (_buffer)
            {
                while (IsEmpty)
                {
                    Monitor.Wait(_buffer);
                }
                T item = null;
                (item, _buffer[_tail]) = (_buffer[_tail], item);
                Increment(ref _tail);
                return item;
            }
        }
    }
}
