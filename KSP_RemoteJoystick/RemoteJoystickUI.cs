using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP_RemoteJoystick
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class RemoteJoystickUI : MonoBehaviour
    {
        //todo : add ui
        public static bool showUI = false;
        public static string[] inputModes = { "Auto", "OnAutopilot", "OnFlyByWire" };
        public static int inputMode = 0;

        public Rect windowRect;

        Vector2 paddings = new Vector2(10, 10);
        float space = 5;
        float regularHeight = 30;

        public void Start()
        {
            windowRect = new Rect(200, 200, 200, 400);
        }

        public void Update()
        {

        }

        public void OnGUI()
        {
            GUI.Window(233333 ^ 63243652, windowRect, Window, "Remote Joystick");
        }

        public void Window(int wid)
        {
            Vector2 regularSize = new Vector2(windowRect.width - paddings.x * 2, regularHeight);
            Vector2 interval = new Vector2(0, regularSize.y + space);
            Vector2 current = paddings + new Vector2(0, 20);

            GUI.DragWindow();

            if (GUI.Button(new Rect(current, regularSize), RemoteJoystick.isOn ? "Stop Server" : "Start Server"))
            {
                RemoteJoystick.instance.Toggle();
            }
            current += interval;
            current += interval;

            GUI.Label(new Rect(current, regularSize), "Input Mode");
            current += interval;
            if (GUI.Button(new Rect(current, regularSize), inputModes[inputMode]))
            {
                inputMode++;
                if(inputMode >= inputModes.Length)
                {
                    inputMode = 0;
                }
            }
            current += interval;

            windowRect.height = current.y + paddings.y;
        }

    }
}
