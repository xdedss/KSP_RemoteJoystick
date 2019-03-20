using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace KSP_RemoteJoystick
{
    static class Ext
    {
        public static bool GetGroup(this ActionGroupList actions, KSPActionGroup group)
        {
            return actions.groups[BaseAction.GetGroupIndex(group)];
        }

        public static bool SetIfNot(this ActionGroupList actions, KSPActionGroup group, bool value)
        {
            if(actions.GetGroup(group) ^ value)
            {
                actions.ToggleGroup(group);
                return true;
            }
            return false;
        }
    }
}
