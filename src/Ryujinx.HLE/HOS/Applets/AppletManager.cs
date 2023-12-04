using Ryujinx.HLE.HOS.Applets.Browser;
using Ryujinx.HLE.HOS.Applets.Error;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Applets
{
    static class AppletManager
    {
        private static readonly Dictionary<AppletId, Type> _appletMapping;

        static AppletManager()
        {
            _appletMapping = new Dictionary<AppletId, Type>
            {
                { AppletId.Error,            typeof(ErrorApplet)            },
                { AppletId.PlayerSelect,     typeof(PlayerSelectApplet)     },
                { AppletId.Controller,       typeof(ControllerApplet)       },
                { AppletId.SoftwareKeyboard, typeof(SoftwareKeyboardApplet) },
                { AppletId.NetConnect,       typeof(NetConnectApplet)       },
                { AppletId.LibAppletWeb,     typeof(BrowserApplet)          },
                { AppletId.LibAppletShop,    typeof(BrowserApplet)          },
                { AppletId.LibAppletOff,     typeof(BrowserApplet)          },
            };
        }

        public static IApplet Create(AppletId applet, Horizon system)
        {
            switch (applet)
            {
                case AppletId.Controller:
                    return new ControllerApplet(system);
                case AppletId.Error:
                    return new ErrorApplet(system);
                case AppletId.PlayerSelect:
                    return new PlayerSelectApplet(system);
                case AppletId.SoftwareKeyboard:
                    return new SoftwareKeyboardApplet(system);
                case AppletId.LibAppletWeb:
                    return new BrowserApplet(system);
                case AppletId.LibAppletShop:
                    return new BrowserApplet(system);
                case AppletId.LibAppletOff:
                    return new BrowserApplet(system);
            }

            throw new NotImplementedException($"{applet} applet is not implemented.");
        }
    }
}
