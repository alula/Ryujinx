using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Ovln
{
    class OvlnResult
    {
        private const int ModuleId = 25;

        public static Result Success => new(ModuleId, 0);
        public static Result QueueFull => new(ModuleId, 10);
        public static Result NoMessages => new(ModuleId, 12);
    }
}
