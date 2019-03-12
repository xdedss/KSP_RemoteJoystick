﻿using KSP.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP_RemoteJoystick
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class RemoteJoystick : MonoBehaviour
    {
        static bool isOn;

        static bool HasAddedButton;
        static ApplicationLauncherButton launcherButton;
        SocketServer server;
        ushort port = 23333;

        Vessel targetVessel;
        SocketDataParser.ClientSideSocketData lastReceivedData; 

        Texture2D texIdle;
        Texture2D texWaiting;
        Texture2D texWorking;

        bool stage_;

        void Start()
        {
            texIdle = GameDatabase.Instance.GetTexture("RemoteJoystick/Textures/icon_idle", false);
            texWaiting = GameDatabase.Instance.GetTexture("RemoteJoystick/Textures/icon_waiting", false);
            texWorking = GameDatabase.Instance.GetTexture("RemoteJoystick/Textures/icon_working", false);
            if (!HasAddedButton)
            {
                launcherButton = ApplicationLauncher.Instance.AddModApplication(Enabled, Disabled, null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT, texIdle);
                launcherButton.onRightClick = RightClick;
                HasAddedButton = true;
            }
            if (server == null)
            {
                server = new SocketServer(port);
            }
            
            CheckStatus();
            StartCoroutine(CheckTexLoop());
        }

        void OnDestroy()
        {
            if(server != null) {
                server.Close();
            }
        }

        void OnAutopilot(FlightCtrlState s)
        {
            if (!targetVessel.ActionGroups.GetGroup(KSPActionGroup.SAS)) {
                ApplyControl(s);
            }
        }

        void OnFlyByWire(FlightCtrlState s)
        {
            if (targetVessel.ActionGroups.GetGroup(KSPActionGroup.SAS))
            {
                ApplyControl(s);
            }
        }

        void ApplyControl(FlightCtrlState s)
        {
            if (isOn && server.hasClient && lastReceivedData != null)
            {
                s.pitch += -lastReceivedData.joystickR.y;
                s.yaw += lastReceivedData.joystickL.x;
                s.roll += lastReceivedData.joystickR.x;
                s.wheelSteer += -lastReceivedData.steering;
                s.mainThrottle = lastReceivedData.throttle;

                var actions = targetVessel.ActionGroups;
                actions.SetIfNot(KSPActionGroup.SAS, lastReceivedData.SAS);
                actions.SetIfNot(KSPActionGroup.RCS, lastReceivedData.RCS);
                actions.SetIfNot(KSPActionGroup.Brakes, lastReceivedData.brake);
                actions.SetIfNot(KSPActionGroup.Light, lastReceivedData.light);
                actions.SetIfNot(KSPActionGroup.Gear, lastReceivedData.gear);
                for (int i = 0; i < lastReceivedData.actions.Length; i++)
                {
                    var group = (KSPActionGroup)(1 << (i + 6));
                    var value = lastReceivedData.actions[i];
                    //actions.ToggleGroup(group);
                    actions.SetIfNot(group, value);
                }
                if (IsFlipped(ref stage_, lastReceivedData.stage)) StageManager.ActivateNextStage();
            }
        }

        void Update()
        {
            var activeVessel = FlightGlobals.ActiveVessel;
            if (activeVessel != null)
            {
                if (activeVessel != targetVessel)
                {
                    if (targetVessel == null)
                    {
                        targetVessel = activeVessel;
                    }
                    else
                    {
                        targetVessel.OnPostAutopilotUpdate -= OnAutopilot;
                        targetVessel.OnFlyByWire -= OnFlyByWire;
                    }
                    targetVessel = activeVessel;
                    targetVessel.OnPostAutopilotUpdate += OnAutopilot;
                    targetVessel.OnFlyByWire += OnFlyByWire;
                }
            }
        }

        void FixedUpdate()
        {
            server.Update(Time.fixedDeltaTime);
            HandleDataReceived();
            UpdateDataToSend();
            UpdateInitialData();
        }

        void HandleDataReceived()
        {
            if (server.listening && server.dataReceived.Length > 0)
            {
                var data = new SocketDataParser.ClientSideSocketData(server.dataReceived);
                lastReceivedData = data;
            }
        }

        void UpdateDataToSend()
        {
            var data = new SocketDataParser.ServerSideSocketData();
            if (targetVessel != null)
            {
                data.longitude = targetVessel.longitude;
                data.latitude = targetVessel.latitude;
                Vector3 up = targetVessel.mainBody.GetSurfaceNVector(targetVessel.latitude, targetVessel.longitude);
                Vector3 northPole = new Vector3(0, 1, 0);
                Vector3 west = Vector3.Cross(northPole, up);
                Vector3 north = Vector3.Cross(up, west);
                var rotationRef = Quaternion.LookRotation(north, up);
                var rotationRefInv = Quaternion.Inverse(rotationRef);
                var falseRotation = rotationRefInv * targetVessel.ReferenceTransform.rotation;
                var trueForward = falseRotation * new Vector3(0, 1, 0);
                var trueUp = falseRotation * new Vector3(0, 0, -1);
                data.rotation = Quaternion.LookRotation(trueForward, trueUp);
                data.srfVel = rotationRefInv * targetVessel.srf_velocity;
            }

            server.dataToSend = data;
        }

        void UpdateInitialData()
        {
            var data = new SocketDataParser.ServerSideInitialData();
            data.SAS = targetVessel.ActionGroups.GetGroup(KSPActionGroup.SAS);
            data.RCS = targetVessel.ActionGroups.GetGroup(KSPActionGroup.RCS);
            data.brake = targetVessel.ActionGroups.GetGroup(KSPActionGroup.Brakes);
            data.light = targetVessel.ActionGroups.GetGroup(KSPActionGroup.Light);
            data.gear = targetVessel.ActionGroups.GetGroup(KSPActionGroup.Gear);
            data.throttle = targetVessel.ctrlState.mainThrottle;
            data.stage = stage_;
            server.initialData = data;
        }

        void CheckStatus()
        {
            if (isOn)
            {
                server.StartListen();
            }
            else
            {
                server.Close();
            }
            CheckTex();
        }

        void CheckTex()
        {
            launcherButton.SetTexture(isOn ? server.hasClient ? texWorking : texWaiting : texIdle);
        }

        void RightClick()
        {
            isOn = !isOn;
            CheckStatus();
            ScreenMessages.PostScreenMessage("RemoteJoystick " + (isOn ? "enabled at " + port : "disabled"), 1f, ScreenMessageStyle.UPPER_CENTER);
        }

        void Enabled()
        {
            //ScreenMessages.PostScreenMessage("RJ ui enabled", 3f, ScreenMessageStyle.UPPER_CENTER);
            ScreenMessages.PostScreenMessage("Rightclick to toggle", 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        void Disabled()
        {
            //ScreenMessages.PostScreenMessage("RJ ui disabled", 3f, ScreenMessageStyle.UPPER_CENTER);
            ScreenMessages.PostScreenMessage("Rightclick to toggle", 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        bool IsFlipped(ref bool stored, bool current)
        {
            if(stored ^ current)
            {
                stored = current;
                return true;
            }
            return false;
        }

        IEnumerator CheckTexLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                CheckTex();
            }
        }
    }
}