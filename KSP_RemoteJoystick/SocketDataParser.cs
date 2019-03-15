using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP_RemoteJoystick
{
    static class SocketDataParser
    {

        public class ClientSideSocketData
        {
            public ClientSideSocketData() { actions = new bool[10]; }
            public ClientSideSocketData(byte[] bytes)
            {
                joystickL = new Vector2(((float)bytes[0]) / 255 * 2 - 1, ((float)bytes[1]) / 255 * 2 - 1);
                joystickR = new Vector2(((float)bytes[2]) / 255 * 2 - 1, ((float)bytes[3]) / 255 * 2 - 1);
                throttle = ((float)bytes[4]) / 255;
                steering = ((float)bytes[5]) / 255 * 2 - 1;
                actions = new bool[10];
                for (int i = 0; i < 8; i++)
                {
                    if ((bytes[6] & ByteMask(i)) != 0)
                    {
                        actions[i] = true;
                    }
                }
                for (int i = 8; i < 10; i++)
                {
                    if ((bytes[7] & ByteMask(i - 8)) != 0)
                    {
                        actions[i] = true;
                    }
                }
                SAS = (bytes[7] & ByteMask(2)) != 0;
                RCS = (bytes[7] & ByteMask(3)) != 0;
                brake = (bytes[7] & ByteMask(4)) != 0;
                light = (bytes[7] & ByteMask(5)) != 0;
                gear = (bytes[7] & ByteMask(6)) != 0;
                abort = (bytes[7] & ByteMask(7)) != 0;
                stage = (bytes[8] & ByteMask(0)) != 0;
                timeWarpMore = (bytes[8] & ByteMask(1)) != 0;
                timeWarpLess = (bytes[8] & ByteMask(2)) != 0;
                map = (bytes[8] & ByteMask(3)) != 0;
                byte controlFlags = bytes[9];
                controlMode = (ControlMode)controlFlags;
            }
            public byte[] ToBytes()
            {
                var j1 = (joystickL + Vector2.one) / 2 * 255;
                var j2 = (joystickR + Vector2.one) / 2 * 255;
                var throttleB = throttle * 255;
                var steeringB = (steering + 1) / 2 * 255;
                int controlFlags = (int)controlMode;
                byte mask1 = 0;
                byte mask2 = 0;
                byte mask3 = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (actions[i])
                    {
                        mask1 |= ByteMask(i);
                    }
                }
                for (int i = 8; i < 10; i++)
                {
                    if (actions[i])
                    {
                        mask2 |= ByteMask(i - 8);
                    }
                }
                if (SAS) mask2 |= ByteMask(2);
                if (RCS) mask2 |= ByteMask(3);
                if (brake) mask2 |= ByteMask(4);
                if (light) mask2 |= ByteMask(5);
                if (gear) mask2 |= ByteMask(6);
                if (abort) mask2 |= ByteMask(7);
                if (stage) mask3 |= ByteMask(0);
                if (timeWarpMore) mask3 |= ByteMask(1);
                if (timeWarpLess) mask3 |= ByteMask(2);
                if (map) mask3 |= ByteMask(3);
                var bytes = new byte[]
                {
                    (byte)Mathf.RoundToInt(j1.x),
                    (byte)Mathf.RoundToInt(j1.y),
                    (byte)Mathf.RoundToInt(j2.x),
                    (byte)Mathf.RoundToInt(j2.y),
                    (byte)Mathf.RoundToInt(throttleB),
                    (byte)Mathf.RoundToInt(steeringB),
                    mask1,
                    mask2,
                    mask3,
                    (byte)controlFlags,
                };
                return bytes;
            }
            public static implicit operator byte[] (ClientSideSocketData data)
            {
                return data.ToBytes();
            }
            public ControlMode controlMode;
            public Vector2 joystickL;
            public Vector2 joystickR;
            public float throttle;
            public float steering;
            public bool[] actions;
            public bool SAS;
            public bool RCS;
            public bool brake;
            public bool light;
            public bool gear;
            public bool abort;
            public bool stage;
            public bool timeWarpMore;
            public bool timeWarpLess;
            public bool map;
        }
        public class ServerSideSocketData
        {
            public ServerSideSocketData() { }
            public ServerSideSocketData(byte[] bytes)
            {
                srfVel = new Vector3(BitConverter.ToSingle(bytes, 0), BitConverter.ToSingle(bytes, 4), BitConverter.ToSingle(bytes, 8));
                var rotX = (float)BitConverter.ToUInt16(bytes, 12) / 65535 * 2 - 1;
                var rotY = (float)BitConverter.ToUInt16(bytes, 14) / 65535 * 2 - 1;
                var rotZ = (float)BitConverter.ToUInt16(bytes, 16) / 65535 * 2 - 1;
                var rotW = (float)BitConverter.ToUInt16(bytes, 18) / 65535 * 2 - 1;
                rotation = new Quaternion(rotX, rotY, rotZ, rotW);
                longitude = (double)BitConverter.ToUInt32(bytes, 20) / uint.MaxValue * 360 - 180;
                latitude = (double)BitConverter.ToUInt32(bytes, 24) / uint.MaxValue * 180 - 90;
                altitudeSealevel = (float)half.FromBytes(bytes, 28);//TODO: display altitude
                altitudeRadar = (float)half.FromBytes(bytes, 30);
            }
            public byte[] ToBytes()
            {
                var velX = BitConverter.GetBytes(srfVel.x);
                var velY = BitConverter.GetBytes(srfVel.y);
                var velZ = BitConverter.GetBytes(srfVel.z);
                var rotX = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.x + 1) / 2 * 65535));
                var rotY = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.y + 1) / 2 * 65535));
                var rotZ = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.z + 1) / 2 * 65535));
                var rotW = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.w + 1) / 2 * 65535));

                var lon = BitConverter.GetBytes((uint)Math.Round((longitude / 360 + 0.5) * uint.MaxValue));
                var lat = BitConverter.GetBytes((uint)Math.Round((latitude / 180 + 0.5) * uint.MaxValue));

                var altSL = ((half)altitudeSealevel).GetBytes();
                var altR = ((half)altitudeRadar).GetBytes();

                return new byte[] {
                    velX[0], velX[1], velX[2], velX[3],//0-3
                    velY[0], velY[1], velY[2], velY[3],//4-7
                    velZ[0], velZ[1], velZ[2], velZ[3],//8-11
                    rotX[0], rotX[1],//12 13
                    rotY[0], rotY[1],//14 15
                    rotZ[0], rotZ[1],//16 17
                    rotW[0], rotW[1],//18 19
                    lon[0], lon[1], lon[2], lon[3],//20-23
                    lat[0], lat[1], lat[2], lat[3],//24-27
                    altSL[0], altSL[1],//28 29
                    altR[0], altR[1],//30 31
                };
            }
            public static implicit operator byte[] (ServerSideSocketData data)
            {
                return data.ToBytes();
            }
            public Vector3 srfVel;
            public Quaternion rotation;
            public double longitude;
            public double latitude;
            public float altitudeSealevel;
            public float altitudeRadar;
        }

        public class ServerSideInitialData
        {
            public ServerSideInitialData() { }
            public ServerSideInitialData(byte[] bytes)
            {
                var throttleValue = (float)bytes[0] / 255;
                throttle = throttleValue;
                SAS = (bytes[1] & ByteMask(0)) != 0;
                RCS = (bytes[1] & ByteMask(1)) != 0;
                brake = (bytes[1] & ByteMask(2)) != 0;
                light = (bytes[1] & ByteMask(3)) != 0;
                gear = (bytes[1] & ByteMask(4)) != 0;
                stage = (bytes[1] & ByteMask(5)) != 0;
            }
            public byte[] ToBytes()
            {
                byte throttleb = (byte)Mathf.RoundToInt(throttle * 255);
                byte flags = 0;
                if (SAS) flags |= ByteMask(0);
                if (RCS) flags |= ByteMask(1);
                if (brake) flags |= ByteMask(2);
                if (light) flags |= ByteMask(3);
                if (gear) flags |= ByteMask(4);
                if (stage) flags |= ByteMask(5);
                return new byte[] { throttleb, flags };
            }
            public static implicit operator byte[] (ServerSideInitialData data)
            {
                return data.ToBytes();
            }
            public float throttle;
            public bool SAS;
            public bool RCS;
            public bool brake;
            public bool light;
            public bool gear;
            public bool stage;
        }

        public enum ControlMode
        {
            Rotation = 0,
            Docking = 1,
            Rover = 2,
        }

        static byte ByteMask(int pos)
        {
            return (byte)(1 << pos);
        }

    }
}
