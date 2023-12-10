using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types;
using System;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService
{
    class INetworkProfile : IpcService
    {
        public UInt128 Uuid { get; private set; }
        public NetworkProfileData ProfileData;

        public INetworkProfile(UInt128 uuid, NetworkProfileData profileData)
        {
            Uuid = uuid;
            ProfileData = profileData;
        }

        [CommandCmif(0)]
        // Update(buffer<nn::nifm::detail::sf::NetworkProfileData, 0x19>) -> nn::util::Uuid;
        public ResultCode Update(ServiceCtx context)
        {
            ulong networkProfileDataPosition = context.Request.PtrBuff[0].Position;
            NetworkProfileData networkProfile = context.Memory.Read<NetworkProfileData>(networkProfileDataPosition);

            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { networkProfile });

            ProfileData = networkProfile;

            context.ResponseData.WriteStruct(Uuid);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // Persist(nn::util::Uuid) -> nn::util::Uuid;
        public ResultCode Persist1(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { uuid });

            Uuid = uuid;

            context.ResponseData.WriteStruct(Uuid);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // Persist() -> nn::util::Uuid;
        public ResultCode Persist2(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            context.ResponseData.WriteStruct(Uuid);

            return ResultCode.Success;
        }
    }
}
