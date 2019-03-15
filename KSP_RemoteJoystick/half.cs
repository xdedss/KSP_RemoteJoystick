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

        private half(ushort encoded)
        {
            encodedValue = encoded;
        }

        public static half FromFloat(double f)
        {
            if (f <= 0)
            {
                return new half(0);
            }
            float log = (float)Math.Log(f);
            if (log < -5) log = -5;
            if (log > 25) log = 25;

            float coded = (log + 5) / 30 * 65535;
            return new half((ushort)Math.Round(coded));
        }

        public static explicit operator half(double f)
        {
            return half.FromFloat(f);
        }

        public static explicit operator float(half h)
        {
            return (float)Math.Exp(((float)h.encodedValue) / 65535 * 30 - 5);
        }

        public static half operator *(half l, half r)
        {
            return half.FromUShort((ushort)(l.encodedValue + r.encodedValue));
        }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(encodedValue);
        }

        public static half FromBytes(byte[] bytes)
        {
            return half.FromUShort(BitConverter.ToUInt16(bytes, 0));
        }

        public static half FromUShort(ushort encoded)
        {
            return new half(encoded);
        }

        public static half FromBytes(byte[] bytes, int startFrom)
        {
            return new half(BitConverter.ToUInt16(bytes, startFrom));
        }

        public override string ToString()
        {
            return ((float)this).ToString();
        }
    }
}
