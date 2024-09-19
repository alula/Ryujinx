using LibHac.Util;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Applets.Browser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class ILibraryAppletSelfAccessor : IpcService
    {
        private readonly AppletStandalone _appletStandalone = new();
        private RealApplet _realApplet;

        public ILibraryAppletSelfAccessor(ServiceCtx context)
        {
            if (context.Device.Processes.ActiveApplication.RealAppletInstance != null)
            {
                _realApplet = context.Device.Processes.ActiveApplication.RealAppletInstance;
            }
            else if (context.Device.Processes.ActiveApplication.ProgramId == 0x0100000000001009)
            {
                // Create MiiEdit data.
                _appletStandalone = new AppletStandalone()
                {
                    AppletId = AppletId.MiiEdit,
                    LibraryAppletMode = LibraryAppletMode.AllForeground,
                };

                byte[] miiEditInputData = new byte[0x100];
                miiEditInputData[0] = 0x03; // Hardcoded unknown value.

                _appletStandalone.InputData.AddLast(miiEditInputData);
            }
            else if (context.Device.Processes.ActiveApplication.ProgramId == 0x010000000000100a)
            {
                _appletStandalone = new AppletStandalone()
                {
                    AppletId = AppletId.LibAppletWeb,
                    LibraryAppletMode = LibraryAppletMode.AllForeground,
                };

                // version = 0x00080000;
                CommonArguments commonArguments = new()
                {
                    Version = 1,
                    StructureSize = (uint)Marshal.SizeOf(typeof(CommonArguments)),
                    AppletVersion = 0x00080000,
                };
                using MemoryStream stream = MemoryStreamManager.Shared.GetStream();
                using BinaryWriter writer = new(stream);
                writer.WriteStruct(commonArguments);

                // todo
                List<BrowserArgument> arguments = new()
                {
                    new BrowserArgument(WebArgTLVType.UnknownFlag0xD, BitConverter.GetBytes(true)),
                    new BrowserArgument(WebArgTLVType.InitialURL, Encoding.UTF8.GetBytes("https://www.google.com/")),
                    new BrowserArgument(WebArgTLVType.Whitelist, Encoding.UTF8.GetBytes("^http*"))
                };

                var argumentData = BrowserArgument.BuildArguments(ShimKind.Web, arguments);

                _appletStandalone.InputData.AddLast(stream.ToArray());
                _appletStandalone.InputData.AddLast(argumentData);
            }
            else if (context.Device.Processes.ActiveApplication.ProgramId == 0x010000000000100d)
            {
                _appletStandalone = new AppletStandalone()
                {
                    AppletId = AppletId.PhotoViewer,
                    LibraryAppletMode = LibraryAppletMode.AllForeground,
                };

                // version = 0x00080000;
                CommonArguments commonArguments = new()
                {
                    Version = 1,
                    StructureSize = (uint)Marshal.SizeOf(typeof(CommonArguments)),
                    AppletVersion = 0x1,
                    PlayStartupSound = true,
                };
                using MemoryStream stream = MemoryStreamManager.Shared.GetStream();
                using BinaryWriter writer = new(stream);
                writer.WriteStruct(commonArguments);

                _appletStandalone.InputData.AddLast(stream.ToArray());
                _appletStandalone.InputData.AddLast(new byte[1] { 2 });
            }
            else
            {
                throw new NotImplementedException($"{context.Device.Processes.ActiveApplication.ProgramId:X16} applet is not implemented.");
            }
        }

        [CommandCmif(0)]
        // PopInData() -> object<nn::am::service::IStorage>
        public ResultCode PopInData(ServiceCtx context)
        {
            byte[] appletData;

            if (_realApplet != null)
            {
                if (!_realApplet.NormalSession.TryPop(out appletData))
                {
                    return ResultCode.NotAvailable;
                }
            }
            else
            {
                appletData = _appletStandalone.InputData.First.Value;
                _appletStandalone.InputData.RemoveFirst();
            }

            if (appletData.Length == 0)
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        public ResultCode PushOutData(ServiceCtx context)
        {
            if (_realApplet != null)
            {
                IStorage data = GetObject<IStorage>(context, 0);
                _realApplet.NormalSession.Push(data.Data);
                _realApplet.InvokeAppletStateChanged();
            }

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        public ResultCode ExitProcessAndReturn(ServiceCtx context)
        {
            context.Process.Terminate();
            context.Device.System.ReturnFocus();

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // GetLibraryAppletInfo() -> nn::am::service::LibraryAppletInfo
        public ResultCode GetLibraryAppletInfo(ServiceCtx context)
        {
            LibraryAppletInfo libraryAppletInfo = new();

            if (_realApplet != null)
            {
                libraryAppletInfo.AppletId = _realApplet.AppletId;
                libraryAppletInfo.LibraryAppletMode = LibraryAppletMode.AllForeground; // TODO
            }
            else
            {
                libraryAppletInfo.AppletId = _appletStandalone.AppletId;
                libraryAppletInfo.LibraryAppletMode = _appletStandalone.LibraryAppletMode;
            }

            context.ResponseData.WriteStruct(libraryAppletInfo);

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // GetMainAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetMainAppletIdentityInfo(ServiceCtx context)
        {
            AppletIdentifyInfo appletIdentifyInfo = new()
            {
                AppletId = AppletId.QLaunch,
                TitleId = 0x0100000000001000,
            };

            context.ResponseData.WriteStruct(appletIdentifyInfo);

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // CanUseApplicationCore() -> bool
        public ResultCode CanUseApplicationCore(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(14)]
        // GetCallerAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetCallerAppletIdentityInfo(ServiceCtx context)
        {
            AppletIdentifyInfo appletIdentifyInfo = new()
            {
                AppletId = AppletId.QLaunch,
                TitleId = 0x0100000000001000,
            };

            context.ResponseData.WriteStruct(appletIdentifyInfo);

            return ResultCode.Success;
        }

        [CommandCmif(30)]
        // UnpopInData(nn::am::service::IStorage)
        public ResultCode UnpopInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            if (_realApplet != null)
            {
                _realApplet.NormalSession.InsertFront(data.Data);
            }
            else
            {
                _appletStandalone.InputData.AddFirst(data.Data);
            }

            return ResultCode.Success;
        }

        [CommandCmif(50)]
        // ReportVisibleError(nn::err::ErrorCode)
        public ResultCode ReportVisibleError(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
    }
}
