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

            GUI.Label(new Rect(current, regularSize), "test");
            current += interval;
            GUI.Button(new Rect(current, regularSize), "testbutton");
            current += interval;

            windowRect.height = current.y + paddings.y;
        }

    }
}
