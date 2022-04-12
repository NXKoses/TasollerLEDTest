using System;
using System.Runtime.InteropServices;

namespace TasollerLED
{
    internal class Timer
    {
        [DllImport("winmm.dll", SetLastError = true)]
        static extern UInt32 timeSetEvent(UInt32 msDelay, UInt32 msResolution, TimerEventHandler handler, ref UInt32 userCtx, UInt32 eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        static extern UInt32 timeKillEvent(UInt32 timerEventId);

        [DllImport("winmm.dll")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll")]
        public static extern uint timeEndPeriod(uint uMilliseconds);

        [DllImport("kernel32.dll")]
        static extern void InitializeCriticalSection(out CRITICAL_SECTION lpCriticalSection);

        [DllImport("kernel32.dll")]
        static extern void EnterCriticalSection(ref CRITICAL_SECTION lpCriticalSection);

        [DllImport("kernel32.dll")]
        static extern void LeaveCriticalSection(ref CRITICAL_SECTION lpCriticalSection);

        private delegate void TimerEventHandler(UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2);

        [StructLayout(LayoutKind.Sequential)]
        public struct CRITICAL_SECTION
        {
            public IntPtr DebugInfo;
            public long LockCount;
            public long RecursionCount;
            public uint OwningThread;
            public uint LockSemaphore;
            public int Reserved;
        }
        const int TIMERR_NOERROR = 0;
        const uint TIME_PERIODIC = 1;

        struct callBackParam
        {
            int value;
        }

        public delegate void OnTimerEventHandler();

        private CRITICAL_SECTION CriticalSection;
        private uint TimerID = 0;
        TimerEventHandler teh;

        public void UpdateTimer(uint delay, uint resolution = 100)
        {
            teh = TimerProc;
            if (TimerID != 0)
            {
                timeKillEvent(TimerID);
            }

            if (timeBeginPeriod(resolution) == TIMERR_NOERROR)
            {
                uint userctx = 0;
                TimerID = timeSetEvent(delay, resolution, teh, ref userctx, TIME_PERIODIC);
            }
        }

        public void StopTimer()
        {
            if (TimerID != 0)
            {
                uint resolution = 1;
                timeKillEvent(TimerID);
                timeEndPeriod(resolution);
            }
        }

        private void TimerProc(UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2)
        {
            InitializeCriticalSection(out CriticalSection);
            EnterCriticalSection(ref CriticalSection);
            try
            {
                OnTimerEventHandler onTimerEvent1 = Padlight.SliderTick;
                OnTimerEventHandler onTimerEvent2 = Padlight.AirTick;
                OnTimerEventHandler onTimerEvent3 = Padlight.WriteColorTick;
                OnTimerEventHandler onTimerEvent4 = Padlight.ReadTick;
                onTimerEvent1.Invoke();
                onTimerEvent2.Invoke();
                onTimerEvent3.Invoke();
                onTimerEvent4.Invoke();
            }
            finally
            {
                LeaveCriticalSection(ref CriticalSection);
            }
        }
    }
}
