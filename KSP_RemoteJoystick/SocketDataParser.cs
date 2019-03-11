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
                stage = (bytes[7] & ByteMask(7)) != 0;
            }
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
            public bool stage;
        }
        public class ServerSideSocketData
        {
            public ServerSideSocketData()
            {

            }
            public byte[] ToBytes()
            {
                //var eulers = rotation.eulerAngles;
                //var pitch = eulers.x;
                //var roll = eulers.z;
                //var hdg = eulers.y;

                //var pitchs = (ushort)Math.Round((pitch + 90) / 180 * 65535);
                //var rolls = (ushort)Math.Round((roll + 180) / 360 * 65535);
                //var hdgs = (ushort)Math.Round(hdg / 360 * 65535);

                //var pitchb = BitConverter.GetBytes(pitchs);
                //var rollb = BitConverter.GetBytes(rolls);
                //var hdgb = BitConverter.GetBytes(hdgs);
                //var srfVelb = BitConverter.GetBytes(srfVel.magnitude);

                var velX = BitConverter.GetBytes(srfVel.x);
                var velY = BitConverter.GetBytes(srfVel.y);
                var velZ = BitConverter.GetBytes(srfVel.z);
                var rotX = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.x + 1) / 2 * 65535));
                var rotY = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.y + 1) / 2 * 65535));
                var rotZ = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.z + 1) / 2 * 65535));
                var rotW = BitConverter.GetBytes((ushort)Mathf.RoundToInt((rotation.w + 1) / 2 * 65535));
                //Debug.Log(string.Format("parsed{0}-X:{1}", rotation, Mathf.RoundToInt((rotation.x + 1) / 2 * 65535)));

                var lon = BitConverter.GetBytes((uint)Math.Round((longitude / 360 + 0.5) * uint.MaxValue));
                var lat = BitConverter.GetBytes((uint)Math.Round((latitude / 180 + 0.5) * uint.MaxValue));

                var altSL = ((half)altitudeSealevel).GetBytes();
                var altR = ((half)altitudeRadar).GetBytes();

                //byte flags = 0;
                //if (SAS) flags |= ByteMask(0);
                //if (RCS) flags |= ByteMask(1);
                //if (brake) flags |= ByteMask(2);
                //if (light) flags |= ByteMask(3);
                //if (gear) flags |= ByteMask(4);
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
                    
                };
            }
            public static implicit operator byte[](ServerSideSocketData data)
            {
                return data.ToBytes();
            }
            public Vector3 srfVel;
            public Quaternion rotation;
            public double longitude;
            public double latitude;
            public float altitudeSealevel;
            public float altitudeRadar;
            //public bool SAS;
            //public bool RCS;
            //public bool brake;
            //public bool light;
            //public bool gear;
        }

        public class ServerSideInitialData
        {
            public ServerSideInitialData()
            {

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

        static byte ByteMask(int pos)
        {
            return (byte)(1 << pos);
        }
        
    }
}
