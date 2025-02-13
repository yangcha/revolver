using System;
using System.Threading;

namespace Concurrent
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Revolver<T> : IDisposable where T : class, IDisposable
    {
        private readonly T[] _buffer;

        private readonly int _bufferSize;
        private int _head;
        private int _tail;
        private bool disposedValue;

        public bool IsFinished { get; private set; }

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
            IsFinished = false;
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

        public void Finish()
        {
            lock (_buffer)
            {
                IsFinished = true;
                Monitor.PulseAll(_buffer);
            }
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
                Monitor.PulseAll(_buffer);
            }
        }

        /// <summary>
        /// Removes an item from the circular buffer
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            T item = null;
            lock (_buffer)
            {
                while (IsEmpty && !IsFinished)
                {
                    Monitor.Wait(_buffer);
                }
                if (!IsFinished)
                {
                    (item, _buffer[_tail]) = (_buffer[_tail], item);
                    Increment(ref _tail);
                }
            }
            return item;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    lock (_buffer)
                    {
                        for (int i = 0; i < _buffer.Length; i++)
                        {
                            _buffer[i]?.Dispose();
                            _buffer[i] = null;
                        }
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
