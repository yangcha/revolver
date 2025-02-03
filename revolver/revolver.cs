using System;
using System.Threading;


namespace revolver
{
    public class Revolver<T> where T : IDisposable
    {
        private readonly T[] _buffer;

        private readonly int _bufferSize;

        private int _head;
        private int _tail;

        public Revolver(int capacity)
        {
            _head = 0;
            _tail = 0;
            _bufferSize = capacity + 1;
            _buffer = new T[_bufferSize];
        }

        public int Capacity { get { return _bufferSize - 1; } }


        public int Size { get { return (_head - _tail + _bufferSize) % _bufferSize; } }


        private bool IsEmpty { get { return _head == _tail; } }

        private void Increment(ref int value) { value = (value + 1) % _bufferSize; }

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
