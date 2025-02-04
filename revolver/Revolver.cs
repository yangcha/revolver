using System;
using System.Threading;


namespace Concurrent
{
    public class Revolver<T> : IDisposable where T : IDisposable
    {
        private readonly T[] _buffer;

        private readonly int _bufferSize;
        private int _head;
        private int _tail;
        private bool disposedValue;

        private bool IsAddingCompleted { get; set; }

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
            IsAddingCompleted = false;
        }

        /// <summary>
        /// Maximum capacity of the buffer. If the maximum capacity is reached,
        /// then overwrite the next element.
        /// </summary>
        public int Capacity { get { lock (_buffer) { return _bufferSize - 1; } } }

        /// <summary>
        /// The size of buffer
        /// </summary>
        public int Count { get { lock (_buffer) { return (_head - _tail + _bufferSize) % _bufferSize; } } }

        public void CompleteAdding()
        {
            if (IsAddingCompleted)
            {
                return;
            }
            IsAddingCompleted = true;
        }


        /// <summary>
        /// If the buffer is empty.
        /// </summary>
        private bool IsEmpty { get { return _head == _tail; } }

        public bool IsCompleted { get { return IsAddingCompleted;  } }

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
                Monitor.Pulse(_buffer);
            }
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
                var item = default(T);
                (item, _buffer[_tail]) = (_buffer[_tail], item);
                Increment(ref _tail);
                return item;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    for(int i = 0; i < _buffer.Length; i++)
                    {
                        _buffer[i]?.Dispose();
                        _buffer[i] = default;
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
