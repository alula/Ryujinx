using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Srepo
{
    interface ISrepoService : IServiceObject
    {
        Result SaveReport(Span<byte> EventId, ulong ApplicationId, Span<byte> Report);
        Result SaveReportWithUser(Span<byte> EventId, ulong ApplicationId, Span<byte> Report, Uid userId);
        Result SaveReportForAntiPiracy(Span<byte> EventId, ulong ApplicationId, Span<byte> Report);
        Result SaveReportForAntiPiracyWithUser(Span<byte> EventId, ulong ApplicationId, Span<byte> Report, Uid userId);
        Result NotifyUserList(Span<Uid> userIds);
        Result NotifyUserDeleted(Uid userId);
        Result NotifyUserRegistered(Uid userId);
        Result NotifyUserClosed(Uid userId);
        Result NotifyUserOpened(Uid userId);
        Result NotifyUserClosedWithApplicationId(Uid userId, ulong ApplicationId);
        Result NotifyUserOpenedWithApplicationId(Uid userId, ulong ApplicationId);
    }
}
