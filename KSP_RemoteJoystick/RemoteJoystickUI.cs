using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP_RemoteJoystick
{
    class RemoteJoystickUI : MonoBehaviour
    {
        //todo : add ui
        public bool showUI = false;
        public string[] inputModes = { "Auto", "OnAutopilot", "OnFlyByWire" };
        public bool AutoControl => inputMode == 0;
        public bool OnAutopilotControl => inputMode == 1;
        public bool OnFlyByWireControl => inputMode == 2;
        public int inputMode = 0;
        public RemoteJoystick mod;

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
            if (showUI)
            {
                windowRect = GUI.Window(233333 ^ 63243652, windowRect, Window, "Remote Joystick");
            }
        }

        public void Window(int wid)
        {
            Vector2 regularSize = new Vector2(windowRect.width - paddings.x * 2, regularHeight);
            Vector2 interval = new Vector2(0, regularSize.y + space);
            Vector2 current = paddings + new Vector2(0, 20);

            GUI.DragWindow(new Rect(0, 0, windowRect.x, 20));

            if (GUI.Button(new Rect(current, regularSize), mod.IsOn ? "Stop Server" : "Start Server"))
            {
                mod.Toggle();
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
