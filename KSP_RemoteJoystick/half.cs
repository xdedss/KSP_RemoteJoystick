using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSP_RemoteJoystick
{
    // e^-5 ~ e^25
    struct half
    {
        // oops only positive values are allowed
        ushort encodedValue;

        private half(double f)
        {
            if (f <= 0)
            {
                encodedValue = 0;
                return;
            }
            float log = (float)Math.Log(f);
            if (log < -5) log = -5;
            if (log > 25) log = 25;

            float coded = (log + 5) / 30 * 65535;
            encodedValue = (ushort)Math.Round(coded);
        }

        private half(ushort encoded)
        {
            encodedValue = encoded;
        }

        public static explicit operator half(double f)
        {
            return new half(f);
        }

        public static implicit operator float(half h)
        {
            return (float)Math.Exp(((float)h.encodedValue) / 65535 * 30 - 5);
        }

        public static half operator *(half l, half r)
        {
            return new half(l.encodedValue + r.encodedValue);
        }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(encodedValue);
        }

        public static half FromBytes(byte[] bytes)
        {
            return new half(BitConverter.ToUInt16(bytes, 0));
        }

        public static half FromBytes(byte[] bytes, int startFrom)
        {
            return new half(BitConverter.ToUInt16(bytes, startFrom));
        }
    }
}
