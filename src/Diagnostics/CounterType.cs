using System;

namespace DatabaseQueue.Diagnostics
{
    [Flags]
    public enum CounterType
    {
        None = 0x0,
        EnqueueTime = 0x1,
        EnqueueTimeBase = 0x2,
        EnqueueRate = 0x4,
        EnqueueData = 0x8,
        DequeueTime = 0x10,
        DequeueTimeBase = 0x20,
        DequeueRate = 0x40,
        DequeueData = 0x80,
        Items = 0x100
    }
}
