namespace Ryujinx.Horizon.Sdk.OsTypes
{
    public struct SystemEventType
    {
        public enum InitializationState : byte
        {
            NotInitialized,
            InitializedAsEvent,
            InitializedAsInterProcess,
        }

        public InterProcessEventType InterProcessEvent;
        public InitializationState State;

        public readonly bool NotInitialized => State == InitializationState.NotInitialized;
    }
}
