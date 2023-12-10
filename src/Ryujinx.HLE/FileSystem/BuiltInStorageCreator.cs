using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.FsCreator;

namespace Ryujinx.HLE.FileSystem
{
    public class BuiltInStorageCreator : IBuiltInStorageCreator
    {
        public static byte[] Cal0Blob;

        public Result Create(ref SharedRef<IStorage> outStorage, BisPartitionId partitionId)
        {
            if (partitionId == BisPartitionId.CalibrationBinary && Cal0Blob != null)
            {
                outStorage = new SharedRef<IStorage>(new MemoryStorage(Cal0Blob));
                return Result.Success;
            }

            return ResultFs.PartitionNotFound.Value;
        }

        public Result InvalidateCache()
        {
            return Result.Success;
        }
    }
}
