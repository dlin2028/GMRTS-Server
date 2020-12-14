using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses
{
    public struct Changing<T>
    {
        public ulong StartTime { get; set; }

        public IChanger<T> Changer { get; }

        public T Start { get; set; }
        public T Value { get; set; }
        public T Change { get; set; }

        public Changing(T start, T change, IChanger<T> changer, ulong startMillis)
        {
            Start = start;
            Value = start;
            Change = change;
            Changer = changer;
            StartTime = startMillis;
        }

        public Changing<T> Update(ulong currentMillis)
        {
            // (currentMillis - StartTime) should never be negative but it apparently is sometimes, so I'm casting to long from ulong to sidestep the problem :)
            Value = Changer.Add(Start, Changer.Scale((long)(currentMillis - StartTime) / 1000f, Change));
            return this;
            //Value = Changer.Add(Value, Changer.Scale(elapsedSeconds, Change));
        }
    }
}
