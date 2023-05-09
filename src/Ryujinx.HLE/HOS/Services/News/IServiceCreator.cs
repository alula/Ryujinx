using Ryujinx.HLE.HOS.Services.News.ServiceCreator;

namespace Ryujinx.HLE.HOS.Services.News
{
    [Service("news:a")]
    [Service("news:c")]
    [Service("news:m")]
    [Service("news:p")]
    [Service("news:v")]
    class IServiceCreator : IpcService
    {
        public IServiceCreator(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateNewsService() -> object<nn::news::detail::ipc::INewsService>
        public ResultCode CreateNewsService(ServiceCtx context)
        {
            MakeObject(context, new INewsService(context));

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // CreateNewlyArrivedEventHolder() -> object<nn::news::detail::ipc::INewlyArrivedEventHolder>
        public ResultCode CreateNewlyArrivedEventHolder(ServiceCtx context)
        {
            MakeObject(context, new INewlyArrivedEventHolder(context));

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // CreateNewsDataService() -> object<nn::news::detail::ipc::INewsDataService>
        public ResultCode CreateNewsDataService(ServiceCtx context)
        {
            MakeObject(context, new INewsDataService(context));

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // CreateNewsDatabaseService() -> object<nn::news::detail::ipc::INewsDatabaseService>
        public ResultCode CreateNewsDatabaseService(ServiceCtx context)
        {
            MakeObject(context, new INewsDatabaseService(context));

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // CreateOverwriteEventHolder() -> object<nn::news::detail::ipc::IOverwriteEventHolder>
        public ResultCode CreateOverwriteEventHolder(ServiceCtx context)
        {
            MakeObject(context, new IOverwriteEventHolder(context));

            return ResultCode.Success;
        }
    }
}
