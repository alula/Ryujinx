using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.GeneralService;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types;
using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService
{
    class IGeneralService : DisposableIpcService
    {
        private readonly GeneralServiceDetail _generalServiceDetail;

        private IPInterfaceProperties _targetPropertiesCache = null;
        private UnicastIPAddressInformation _targetAddressInfoCache = null;
        private string _cacheChosenInterface = null;

        private readonly UInt128 _interfaceId = UInt128Utils.CreateRandom();

        public IGeneralService()
        {
            _generalServiceDetail = new GeneralServiceDetail
            {
                ClientId = GeneralServiceManager.Count,
                IsAnyInternetRequestAccepted = true, // NOTE: Why not accept any internet request?
            };

            NetworkChange.NetworkAddressChanged += LocalInterfaceCacheHandler;

            GeneralServiceManager.Add(_generalServiceDetail);
        }

        [CommandCmif(1)]
        // GetClientId() -> buffer<nn::nifm::ClientId, 0x1a, 4>
        public ResultCode GetClientId(ServiceCtx context)
        {
            ulong position = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(sizeof(int));

            context.Memory.Write(position, _generalServiceDetail.ClientId);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // CreateScanRequest() -> object<nn::nifm::detail::IScanRequest>
        public ResultCode CreateScanRequest(ServiceCtx context)
        {
            MakeObject(context, new IScanRequest(context.Device.System));

            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // CreateRequest(u32 version) -> object<nn::nifm::detail::IRequest>
        public ResultCode CreateRequest(ServiceCtx context)
        {
            uint version = context.RequestData.ReadUInt32();

            MakeObject(context, new IRequest(context.Device.System, version));

            // Doesn't occur in our case.
            // return ResultCode.ObjectIsNull;

            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { version });

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // GetCurrentNetworkProfile() -> buffer<nn::nifm::detail::sf::NetworkProfileData, 0x1a, 0x17c>
        public ResultCode GetCurrentNetworkProfile(ServiceCtx context)
        {
            ulong networkProfileDataPosition = context.Request.RecvListBuff[0].Position;

            (IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (interfaceProperties == null || unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            Logger.Info?.Print(LogClass.ServiceNifm, $"Console's local IP is \"{unicastAddress.Address}\".");

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Unsafe.SizeOf<NetworkProfileData>());

            NetworkProfileData networkProfile = new()
            {
                Uuid = _interfaceId,
            };

            networkProfile.IpSettingData.IpAddressSetting = new IpAddressSetting(interfaceProperties, unicastAddress);
            networkProfile.IpSettingData.DnsSetting = new DnsSetting(interfaceProperties);

            "RyujinxNetwork"u8.CopyTo(networkProfile.Name.AsSpan());

            context.Memory.Write(networkProfileDataPosition, networkProfile);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // EnumerateNetworkInterfaces() -> (u32, buffer<nn::nifm::detail::sf::NetworkInterfaceInfo, 0xa>)
        public ResultCode EnumerateNetworkInterfaces(ServiceCtx context)
        {
            context.ResponseData.Write(1); // account crashes if we don't have at least one interface

            // TODO: write interface info

            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [CommandCmif(7)]
        // EnumerateNetworkProfiles() -> (u32, buffer<nn::nifm::detail::sf::NetworkProfileBasicInfo[], 0x6>)
        public ResultCode EnumerateNetworkProfiles(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [CommandCmif(8)]
        // GetNetworkProfile(nn::util::Uuid) -> buffer<nn::nifm::detail::sf::NetworkProfileData, 0x1a>
        public ResultCode GetNetworkProfile(ServiceCtx context)
        {
            ulong networkProfileDataPosition = context.Request.RecvListBuff[0].Position;
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            if (uuid != _interfaceId)
            {
                return ResultCode.NoInternetConnection;
            }

            (IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (interfaceProperties == null || unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            Logger.Info?.Print(LogClass.ServiceNifm, $"Console's local IP is \"{unicastAddress.Address}\".");

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Unsafe.SizeOf<NetworkProfileData>());

            NetworkProfileData networkProfile = new()
            {
                Uuid = _interfaceId,
            };

            networkProfile.IpSettingData.IpAddressSetting = new IpAddressSetting(interfaceProperties, unicastAddress);
            networkProfile.IpSettingData.DnsSetting = new DnsSetting(interfaceProperties);

            "RyujinxNetwork"u8.CopyTo(networkProfile.Name.AsSpan());

            context.Memory.Write(networkProfileDataPosition, networkProfile);

            return ResultCode.Success;
        }

        [CommandCmif(9)]
        // SetNetworkProfile(buffer<nn::nifm::detail::sf::NetworkProfileData, 0x19>)
        public ResultCode SetNetworkProfile(ServiceCtx context)
        {
            ulong networkProfileDataPosition = context.Request.PtrBuff[0].Position;

            NetworkProfileData networkProfile = context.Memory.Read<NetworkProfileData>(networkProfileDataPosition);
            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { networkProfile });

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // RemoveNetworkProfile(nn::util::Uuid)
        public ResultCode RemoveNetworkProfile(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();
            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { uuid });

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // GetScanData(u32) -> (u32, buffer<nn::nifm::detail::sf::AccessPointData, 0xa>)
        public ResultCode GetScanData(ServiceCtx context)
        {
            SystemVersion version = context.Device.System.ContentManager.GetCurrentFirmwareVersion();
            if (version.Major >= 4)
            {
                // TODO: write AccessPointData
            }
            else
            {
                // TOO: write AccessPointDataOld
            }

            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // GetCurrentIpAddress() -> nn::nifm::IpV4Address
        public ResultCode GetCurrentIpAddress(ServiceCtx context)
        {
            (_, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            context.ResponseData.WriteStruct(new IpV4Address(unicastAddress.Address));

            Logger.Info?.Print(LogClass.ServiceNifm, $"Console's local IP is \"{unicastAddress.Address}\".");

            return ResultCode.Success;
        }

        [CommandCmif(14)]
        // CreateTemporaryNetworkProfile(buffer<nn::nifm::detail::sf::NetworkProfileData, 0x19>) -> (nn::util::Uuid, object<nn::nifm::detail::INetworkProfile>)
        public ResultCode CreateTemporaryNetworkProfile(ServiceCtx context)
        {
            ulong networkProfileDataPosition = context.Request.PtrBuff[0].Position;

            NetworkProfileData networkProfile = context.Memory.Read<NetworkProfileData>(networkProfileDataPosition);
            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { networkProfile });

            var uuid = UInt128Utils.CreateRandom();
            var networkProfileObject = new INetworkProfile(uuid, networkProfile);

            context.ResponseData.WriteStruct(uuid);
            MakeObject(context, networkProfileObject);

            return ResultCode.Success;
        }

        [CommandCmif(15)]
        // GetCurrentIpConfigInfo() -> (nn::nifm::IpAddressSetting, nn::nifm::DnsSetting)
        public ResultCode GetCurrentIpConfigInfo(ServiceCtx context)
        {
            (IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (interfaceProperties == null || unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            Logger.Info?.Print(LogClass.ServiceNifm, $"Console's local IP is \"{unicastAddress.Address}\".");

            context.ResponseData.WriteStruct(new IpAddressSetting(interfaceProperties, unicastAddress));
            context.ResponseData.WriteStruct(new DnsSetting(interfaceProperties));

            return ResultCode.Success;
        }

        [CommandCmif(17)]
        // IsWirelessCommunicationEnabled() -> b8
        public ResultCode IsWirelessCommunicationEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(true);

            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [CommandCmif(18)]
        // GetInternetConnectionStatus() -> nn::nifm::detail::sf::InternetConnectionStatus
        public ResultCode GetInternetConnectionStatus(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return ResultCode.NoInternetConnection;
            }

            InternetConnectionStatus internetConnectionStatus = new()
            {
                Type = InternetConnectionType.WiFi,
                WifiStrength = 3,
                State = InternetConnectionState.Connected,
            };

            context.ResponseData.WriteStruct(internetConnectionStatus);

            return ResultCode.Success;
        }

        [CommandCmif(21)]
        // IsAnyInternetRequestAccepted(buffer<nn::nifm::ClientId, 0x19, 4>) -> bool
        public ResultCode IsAnyInternetRequestAccepted(ServiceCtx context)
        {
            ulong position = context.Request.PtrBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong size = context.Request.PtrBuff[0].Size;
#pragma warning restore IDE0059

            int clientId = context.Memory.Read<int>(position);

            context.ResponseData.Write(GeneralServiceManager.Get(clientId).IsAnyInternetRequestAccepted);

            return ResultCode.Success;
        }

        [CommandCmif(26)]
        // SetExclusiveClient(buffer<nn::nifm::ClientId, 0x19, 4>)
        public ResultCode SetExclusiveClient(ServiceCtx context)
        {
            ulong position = context.Request.PtrBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong size = context.Request.PtrBuff[0].Size;
#pragma warning restore IDE0059

            int clientId = context.Memory.Read<int>(position);

            Logger.Stub?.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        private (IPInterfaceProperties, UnicastIPAddressInformation) GetLocalInterface(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return (null, null);
            }

            string chosenInterface = context.Device.Configuration.MultiplayerLanInterfaceId;

            if (_targetPropertiesCache == null || _targetAddressInfoCache == null || _cacheChosenInterface != chosenInterface)
            {
                _cacheChosenInterface = chosenInterface;

                (_targetPropertiesCache, _targetAddressInfoCache) = NetworkHelpers.GetLocalInterface(chosenInterface);
            }

            return (_targetPropertiesCache, _targetAddressInfoCache);
        }

        private void LocalInterfaceCacheHandler(object sender, EventArgs e)
        {
            Logger.Info?.Print(LogClass.ServiceNifm, "NetworkAddress changed, invalidating cached data.");

            _targetPropertiesCache = null;
            _targetAddressInfoCache = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                NetworkChange.NetworkAddressChanged -= LocalInterfaceCacheHandler;

                if (_generalServiceDetail.ClientId < GeneralServiceManager.Count)
                    GeneralServiceManager.Remove(_generalServiceDetail.ClientId);
            }
        }
    }
}
