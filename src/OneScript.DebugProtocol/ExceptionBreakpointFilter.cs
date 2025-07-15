using System;

namespace OneScript.DebugProtocol
{
    [Serializable]
    public class ExceptionBreakpointFilter
    {
        public string Id { get; set; }

        public string Condition { get; set; }
    }
}