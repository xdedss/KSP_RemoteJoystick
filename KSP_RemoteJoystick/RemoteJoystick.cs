using KSP.UI.Screens;
using System;
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

        bool HasAddedButton;
        ApplicationLauncherButton launcherButton;
        SocketServer server;
        ushort port = 23333;

        Vessel targetVessel;
        SocketDataParser.ClientSideSocketData lastReceivedData; 

        Texture2D texEnabled;
        Texture2D texDisabled;

        bool stage_;

        void Start()
        {
            if (!HasAddedButton)
            {
                texEnabled = GameDatabase.Instance.GetTexture("RemoteJoystick/Textures/icon_", false);
                texDisabled = GameDatabase.Instance.GetTexture("RemoteJoystick/Textures/icon", false);
                launcherButton = ApplicationLauncher.Instance.AddModApplication(Enabled, Disabled, null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT, texDisabled);
                launcherButton.onRightClick = RightClick;
                HasAddedButton = true;
            }
            if (server == null)
            {
                server = new SocketServer(port);
            }
            
            CheckStatus();
        }

        void OnDestroy()
        {
            if(server != null) {
                server.Close();
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
                        targetVessel.OnPreAutopilotUpdate -= ApplyControl;
                    }
                    targetVessel = activeVessel;
                    targetVessel.OnPreAutopilotUpdate += ApplyControl;
                }
            }
        }

        void FixedUpdate()
        {
            server.Update();
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
                //var vessel = FlightGlobals.ActiveVessel;
                //vessel.ctrlState.pitch = data.joystickR.y;
            }
        }

        void UpdateDataToSend()
        {
            var data = new SocketDataParser.ServerSideSocketData();
            if (targetVessel != null)
            {
                data.srfVel = (Vector3)targetVessel.srf_velocity;
                data.rotation = targetVessel.srfRelRotation;
                var up = targetVessel.mainBody.GetSurfaceNVector(targetVessel.latitude, targetVessel.longitude);
                //data.SAS = vessel.ctrlState.
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
            launcherButton.SetTexture(isOn ? texEnabled : texDisabled);
        }

        void RightClick()
        {
            isOn = !isOn;
            CheckStatus();
            ScreenMessages.PostScreenMessage("RemoteJoystick " + (isOn ? "enabled at " + port : "disabled"), 1f, ScreenMessageStyle.UPPER_CENTER);
        }

        void Enabled()
        {
            ScreenMessages.PostScreenMessage("RJ ui enabled", 3f, ScreenMessageStyle.UPPER_CENTER);
        }

        void Disabled()
        {
            ScreenMessages.PostScreenMessage("RJ ui disabled", 3f, ScreenMessageStyle.UPPER_CENTER);
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
    }
}