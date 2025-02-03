using System;
using System.Threading;


namespace Concurrent
{
    public class Revolver<T> where T : IDisposable
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
        public int Capacity { get { return _bufferSize - 1; } }

        /// <summary>
        /// The size of buffer
        /// </summary>
        public int Size { get { return (_head - _tail + _bufferSize) % _bufferSize; } }


        /// <summary>
        /// If the buffer is empty.
        /// </summary>
        private bool IsEmpty { get { return _head == _tail; } }

        /// <summary>
        ///  Increments the index variable by one with wrapping around.
        /// </summary>
        /// <param name="value"></param>
        private void Increment(ref int value) { value = (value + 1) % _bufferSize; }

        /// <summary>
        /// Adds the item to circular buffer
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
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
            }
            Monitor.Pulse(_buffer);
        }

        /// <summary>
        /// Removes an item from the circular buffer
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
                var old_tail = _tail;
                Increment(ref _tail);
                return _buffer[old_tail];
            }
        }

    }
}
