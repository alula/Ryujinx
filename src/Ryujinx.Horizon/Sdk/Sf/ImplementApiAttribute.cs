using System;

namespace Ryujinx.Horizon.Sdk.Sf
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class ImplementApiAttribute : Attribute
    {
        public ImplementApiAttribute()
        {
        }
    }
}
