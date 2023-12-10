using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Srepo;
using System;

namespace Ryujinx.Horizon.Srepo.Ipc
{
    partial class SrepoService : ISrepoService
    {
        [CmifCommand(10100)]
        public Result SaveReport([Buffer(HipcBufferFlags.In | HipcBufferFlags.Pointer)] Span<byte> EventId, ulong ApplicationId, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] Span<byte> Report)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(10101)]
        public Result SaveReportWithUser([Buffer(HipcBufferFlags.In | HipcBufferFlags.Pointer)] Span<byte> EventId, ulong ApplicationId, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] Span<byte> Report, Uid userId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(10200)]
        public Result SaveReportForAntiPiracy([Buffer(HipcBufferFlags.In | HipcBufferFlags.Pointer)] Span<byte> EventId, ulong ApplicationId, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] Span<byte> Report)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(10201)]
        public Result SaveReportForAntiPiracyWithUser([Buffer(HipcBufferFlags.In | HipcBufferFlags.Pointer)] Span<byte> EventId, ulong ApplicationId, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] Span<byte> Report, Uid userId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11000)]
        public Result NotifyUserList([Buffer(HipcBufferFlags.In | HipcBufferFlags.Pointer)] Span<Uid> userIds)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11001)]
        public Result NotifyUserDeleted(Uid userId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11002)]
        public Result NotifyUserRegistered(Uid userId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11003)]
        public Result NotifyUserClosed(Uid userId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11004)]
        public Result NotifyUserOpened(Uid userId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11005)]
        public Result NotifyUserClosedWithApplicationId(Uid userId, ulong ApplicationId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }

        [CmifCommand(11006)]
        public Result NotifyUserOpenedWithApplicationId(Uid userId, ulong ApplicationId)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSrepo);
            return Result.Success;
        }
    }
}
