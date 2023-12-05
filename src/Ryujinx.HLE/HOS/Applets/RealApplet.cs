using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Applets
{
    internal class RealApplet : IApplet
    {
        // 0x02 	010000000000100C 	OverlayApplet (overlayDisp)
        // 0x03 	0100000000001000 	SystemAppletMenu (qlaunch)
        // 0x04 	0100000000001012 	SystemApplication (starter)
        // 0x0A 	0100000000001001 	LibraryAppletAuth (auth)
        // 0x0B 	0100000000001002 	LibraryAppletCabinet (cabinet)
        // 0x0C 	0100000000001003 	LibraryAppletController (controller)
        // 0x0D 	0100000000001004 	LibraryAppletDataErase (dataErase)
        // 0x0E 	0100000000001005 	LibraryAppletError (error)
        // 0x0F 	0100000000001006 	LibraryAppletNetConnect (netConnect)
        // 0x10 	0100000000001007 	LibraryAppletPlayerSelect (playerSelect)
        // 0x11 	0100000000001008 	LibraryAppletSwkbd (swkbd)
        // 0x12 	0100000000001009 	LibraryAppletMiiEdit (miiEdit)
        // 0x13 	010000000000100A 	LibraryAppletWeb (web)
        // 0x14 	010000000000100B 	LibraryAppletShop (shop)
        // 0x15 	010000000000100D 	LibraryAppletPhotoViewer (photoViewer)
        // 0x16 	010000000000100E 	LibraryAppletSet (set)
        // 0x17 	010000000000100F 	LibraryAppletOfflineWeb (offlineWeb)
        // 0x18 	0100000000001010 	LibraryAppletLoginShare (loginShare)
        // 0x19 	0100000000001011 	LibraryAppletWifiWebAuth (wifiWebAuth)
        // 0x1A 	0100000000001013 	LibraryAppletMyPage (myPage)
        // 0x1B 	010000000000101A 	LibraryAppletGift (gift)
        // 0x1C 	010000000000101C 	LibraryAppletUserMigration (userMigration)
        // 0x1D 	010000000000101D 	[9.0.0+] LibraryAppletPreomiaSys (EncounterSys)
        // 0x1E 	0100000000001020 	[9.0.0+] LibraryAppletStory (story)
        // 0x1F 	010070000E3C0000 	[9.0.0+] LibraryAppletPreomiaUsr (EncounterUsr)
        // 0x20 	010086000E49C000 	[9.0.0+] LibraryAppletPreomiaUsrDummy (EncounterUsrDummy)
        // 0x21 	0100000000001038 	[10.0.0+] LibraryAppletSample (sample)
        // 0x22 	0100000000001007 	[13.0.0+] LibraryAppletPromoteQualification (playerSelect)
        // 0x32 	010000000000100F 	[17.0.0+] LibraryAppletOfflineWeb (offlineWeb)
        // 0x33 	010000000000100F 	[17.0.0+] LibraryAppletOfflineWeb (offlineWeb)
        // 0x35 	[17.0.0+] 0100000000001010 ([16.0.0-16.1.0] 0100000000001042) 	[17.0.0+] LibraryAppletLoginShare (loginShare) ([16.0.0-16.1.0] )
        // 0x36 	[17.0.0+] 0100000000001010 ([16.0.0-16.1.0] 0100000000001042) 	[17.0.0+] LibraryAppletLoginShare (loginShare) ([16.0.0-16.1.0] )
        // 0x37 	[17.0.0+] 0100000000001010 ([16.0.0-16.1.0] 0100000000001042) 	[17.0.0+] LibraryAppletLoginShare (loginShare) ([16.0.0-16.1.0] ) 
        private static readonly Dictionary<AppletId, ulong> _appletTitles = new Dictionary<AppletId, ulong>
        {
            { AppletId.Error,            0x0100000000001005 },
            { AppletId.NetConnect,       0x0100000000001006 },
            { AppletId.PlayerSelect,     0x0100000000001007 },
            { AppletId.SoftwareKeyboard, 0x0100000000001008 },
            { AppletId.MiiEdit,          0x0100000000001009 },
            { AppletId.LibAppletWeb,     0x010000000000100A },
            { AppletId.LibAppletShop,    0x010000000000100B },
            { AppletId.Controller,       0x010000000000100C },
            { AppletId.PhotoViewer,      0x010000000000100D },
            { AppletId.Settings,         0x010000000000100E },
            { AppletId.LibAppletOff,     0x010000000000100F },
            { AppletId.MyPage,           0x0100000000001013 },
        };

        private readonly Horizon _system;
        private readonly AppletId _appletId;

        private AppletSession _normalSession;
#pragma warning disable IDE0052 // Remove unread private member
        private AppletSession _interactiveSession;
#pragma warning restore IDE0052

        public event EventHandler AppletStateChanged;

        public RealApplet(AppletId appletId, Horizon system)
        {
            _system = system;
            _appletId = appletId;
        }

        public ResultCode Start(AppletSession normalSession, AppletSession interactiveSession)
        {
            _normalSession = normalSession;
            _interactiveSession = interactiveSession;

            // _normalSession.Push(BuildResponse());

            // AppletStateChanged?.Invoke(this, null);

            // _system.ReturnFocus();

            string contentPath = _system.ContentManager.GetInstalledContentPath(_appletTitles[_appletId], StorageId.BuiltInSystem, NcaContentType.Program);

            if (contentPath.Length == 0)
            {
                return ResultCode.NotAllocated;
            }

            if (contentPath.StartsWith("@SystemContent"))
            {
                contentPath = VirtualFileSystem.SwitchPathToSystemPath(contentPath);
            }

            _system.Device.LoadNca(contentPath);

            return ResultCode.Success;
        }

        public ResultCode GetResult()
        {
            return ResultCode.Success;
        }
    }
}
