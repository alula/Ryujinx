using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Account.Acc.AccountService;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    // [Service("acc:u1", AccountServiceFlag.SystemService)] // Max Sessions: 16
    class IAccountServiceForSystemService : IpcService
    {
        private readonly ApplicationServiceServer _applicationServiceServer;

        private readonly KEvent _registrationEvent;
        private readonly KEvent _stateChangeEvent;
        private readonly KEvent _baasAvailabilityChangeEvent;
        private readonly KEvent _profileUpdateEvent;

        public IAccountServiceForSystemService(ServiceCtx context, AccountServiceFlag serviceFlag)
        {
            _applicationServiceServer = new ApplicationServiceServer(serviceFlag);

            _registrationEvent = new KEvent(context.Device.System.KernelContext);
            _stateChangeEvent = new KEvent(context.Device.System.KernelContext);
            _baasAvailabilityChangeEvent = new KEvent(context.Device.System.KernelContext);
            _profileUpdateEvent = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(0)]
        // GetUserCount() -> i32
        public ResultCode GetUserCount(ServiceCtx context)
        {
            return _applicationServiceServer.GetUserCountImpl(context);
        }

        [CommandCmif(1)]
        // GetUserExistence(nn::account::Uid) -> bool
        public ResultCode GetUserExistence(ServiceCtx context)
        {
            return _applicationServiceServer.GetUserExistenceImpl(context);
        }

        [CommandCmif(2)]
        // ListAllUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListAllUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListAllUsers(context);
        }

        [CommandCmif(3)]
        // ListOpenUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListOpenUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListOpenUsers(context);
        }

        [CommandCmif(4)]
        // GetLastOpenedUser() -> nn::account::Uid
        public ResultCode GetLastOpenedUser(ServiceCtx context)
        {
            return _applicationServiceServer.GetLastOpenedUser(context);
        }

        [CommandCmif(5)]
        // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
        public ResultCode GetProfile(ServiceCtx context)
        {
            ResultCode resultCode = _applicationServiceServer.GetProfile(context, out IProfile iProfile);

            if (resultCode == ResultCode.Success)
            {
                MakeObject(context, iProfile);
            }

            return resultCode;
        }

        [CommandCmif(50)]
        // IsUserRegistrationRequestPermitted(pid) -> bool
        public ResultCode IsUserRegistrationRequestPermitted(ServiceCtx context)
        {
            // NOTE: pid is unused.

            return _applicationServiceServer.IsUserRegistrationRequestPermitted(context);
        }

        [CommandCmif(51)]
        // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
        public ResultCode TrySelectUserWithoutInteraction(ServiceCtx context)
        {
            return _applicationServiceServer.TrySelectUserWithoutInteraction(context);
        }

        [CommandCmif(100)]
        // GetUserRegistrationNotifier() -> object<nn::account::detail::INotifier>
        public ResultCode GetUserRegistrationNotifier(ServiceCtx context)
        {

            MakeObject(context, new INotifier(context, _registrationEvent));

            return ResultCode.Success;
        }


        [CommandCmif(101)]
        // GetUserStateChangeNotifier() -> object<nn::account::detail::INotifier>
        public ResultCode GetUserStateChangeNotifier(ServiceCtx context)
        {
            MakeObject(context, new INotifier(context, _stateChangeEvent));

            return ResultCode.Success;
        }

        [CommandCmif(102)]
        // GetBaasAccountManagerForSystemService(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
        public ResultCode GetBaasAccountManagerForSystemService(ServiceCtx context)
        {
            UserId userId = context.RequestData.ReadStruct<UserId>();

            if (userId.IsNull)
            {
                return ResultCode.NullArgument;
            }

            MakeObject(context, new IManagerForSystemService(userId));

            // Doesn't occur in our case.
            // return ResultCode.NullObject;

            return ResultCode.Success;
        }

        [CommandCmif(103)]
        // GetBaasUserAvailabilityChangeNotifier() -> object<nn::account::detail::INotifier>
        public ResultCode GetBaasUserAvailabilityChangeNotifier(ServiceCtx context)
        {
            MakeObject(context, new INotifier(context, _baasAvailabilityChangeEvent));

            return ResultCode.Success;
        }

        [CommandCmif(104)]
        // GetProfileUpdateNotifier() -> object<nn::account::detail::INotifier>
        public ResultCode GetProfileUpdateNotifier(ServiceCtx context)
        {
            MakeObject(context, new INotifier(context, _profileUpdateEvent));

            return ResultCode.Success;
        }

        [CommandCmif(140)] // 6.0.0+
        // ListQualifiedUsers() -> array<nn::account::Uid, 0xa>
        public ResultCode ListQualifiedUsers(ServiceCtx context)
        {
            return _applicationServiceServer.ListQualifiedUsers(context);
        }
    }
}
