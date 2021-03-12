using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Castle.Core.Internal;

namespace JoyLib.Code.Rollers
{
    public class RNG : IRollable
    {
        protected RNGCryptoServiceProvider RandomEngine { get; set; }
        protected byte[] Buffer { get; set; }
        protected int BufferPosition { get; set; }
        public bool IsRandomPoolEnabled { get; protected set; }

        public RNG() : this(true) { }

        public RNG(bool enableRandomPool)
        {
            this.RandomEngine = new RNGCryptoServiceProvider();
            this.IsRandomPoolEnabled = enableRandomPool;
        }

        protected virtual void InitBuffer()
        {
            if (this.IsRandomPoolEnabled)
            {
                if (this.Buffer == null || this.Buffer.Length != 512)
                    this.Buffer = new byte[512];
            }
            else
            {
                if (this.Buffer == null || this.Buffer.Length != 4)
                    this.Buffer = new byte[4];
            }

            this.RandomEngine.GetBytes(this.Buffer);
            this.BufferPosition = 0;
        }

        /// <summary>
        /// Rolls a value between a minimum, and a maximum.
        /// </summary>
        /// <param name="minValue">Inclusive.</param>
        /// <param name="maxValue">Exclusive.</param>
        /// <returns>The integer result of the roll.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Roll(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue));
            }

            if (minValue == maxValue)
            {
                return minValue;
            }

            long diff = maxValue - minValue;

            while (true)
            {
                uint rand = this.GetRandomUInt32();

                long max = 1 + (long)uint.MaxValue;
                long remainder = max % diff;

                if (rand < max - remainder)
                {
                    return (int)(minValue + (rand % diff));
                }
            }
        }

        /// <summary>
        /// Gets one random unsigned 32bit integer in a thread safe manner.
        /// </summary>
        private uint GetRandomUInt32()
        {
            lock (this)
            {
                this.EnsureRandomBuffer(4);

                uint rand = BitConverter.ToUInt32(this.Buffer, this.BufferPosition);

                this.BufferPosition += 4;

                return rand;
            }
        }

        /// <summary>
        /// Ensures that we have enough bytes in the random buffer.
        /// </summary>
        /// <param name="requiredBytes">The number of required bytes.</param>
        private void EnsureRandomBuffer(int requiredBytes)
        {
            if (this.Buffer == null)
            {
                this.InitBuffer();
            }

            if (requiredBytes > this.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredBytes), "cannot be greater than random buffer");
            }

            if (this.Buffer.Length - this.BufferPosition < requiredBytes)
            {
                this.InitBuffer();
            }
        }

        /// <summary>
        /// Roll the successes of a dice pool.
        /// </summary>
        /// <param name="number">The number of d10s to roll.</param>
        /// <param name="threshold">The threshold at which success happens. Inclusive.</param>
        /// <returns>The number of successes.</returns>
        public int RollSuccesses(int number, int threshold)
        {
            int successes = 0;
            for(int i = 0; i < number; i++)
            {
                if(this.Roll(1, 10) >= threshold)
                {
                    successes += 1;
                }
            }
            return successes;
        }

        public T SelectFromCollection<T>(IEnumerable<T> collection)
        {
            if (collection.IsNullOrEmpty())
            {
                return default;
            }

            T[] array = collection.ToArray();
            
            int result = this.Roll(0, array.Length);
            int index = 0;
            T returnItem = default;
            foreach (T item in array)
            {
                if (index == result)
                {
                    returnItem = item;
                    break;
                }

                index++;
            }
            return returnItem;
        }
    }
}
