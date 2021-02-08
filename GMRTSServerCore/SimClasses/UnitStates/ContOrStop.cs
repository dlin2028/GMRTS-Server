using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses.UnitStates
{
    /// <summary>
    /// This is returned by orders to tell the unit what to do with the order.
    /// It can be Continue, Stop, -- you know what, you can read the enum values on your own.
    /// Maybe it should be called ContOrStopOrRequeue.
    /// Or maybe ContStopOrRequeue?
    /// Any way to do commas?
    /// Maybe \usomethingsomething?
    /// </summary>
    public enum ContOrStop
    {
        Continue,
        Stop,
        Requeue
    }
}
