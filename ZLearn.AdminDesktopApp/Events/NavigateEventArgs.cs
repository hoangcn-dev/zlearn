using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.AdminDesktopApp.Enums;

namespace ZLearn.AdminDesktopApp.Events
{
    public class NavigateEventArgs : EventArgs
    {
        public NavigateEventArgs(NavDestination destination)
        {
            Destination = destination;
        }

        public NavDestination Destination { get; }
    }
}
