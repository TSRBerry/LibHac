﻿using System;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.Impl;
using LibHac.FsSrv.Sf;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Spl;
using LibHac.Util;

namespace LibHac.FsSrv
{
    internal class NcaFileSystemService : IRomFileSystemAccessFailureManager, IDisposable
    {
        private const int AocSemaphoreCount = 128;
        private const int RomSemaphoreCount = 10;

        private ReferenceCountedDisposable<NcaFileSystemService>.WeakReference SelfReference { get; set; }
        private NcaFileSystemServiceImpl ServiceImpl { get; }
        private ulong ProcessId { get; }
        private SemaphoreAdaptor AocMountCountSemaphore { get; }
        private SemaphoreAdaptor RomMountCountSemaphore { get; }

        public NcaFileSystemService(NcaFileSystemServiceImpl serviceImpl, ulong processId)
        {
            ServiceImpl = serviceImpl;
            ProcessId = processId;
            AocMountCountSemaphore = new SemaphoreAdaptor(AocSemaphoreCount, AocSemaphoreCount);
            RomMountCountSemaphore = new SemaphoreAdaptor(RomSemaphoreCount, RomSemaphoreCount);
        }

        public Result OpenFileSystemWithPatch(out ReferenceCountedDisposable<IFileSystem> fileSystem,
            ProgramId programId, FileSystemProxyType fsType)
        {
            throw new NotImplementedException();
        }

        public Result OpenCodeFileSystem(out ReferenceCountedDisposable<IFileSystem> fileSystem,
            out CodeVerificationData verificationData, in FspPath path, ProgramId programId)
        {
            throw new NotImplementedException();
        }

        public Result OpenDataFileSystemByCurrentProcess(out ReferenceCountedDisposable<IFileSystem> fileSystem)
        {
            throw new NotImplementedException();
        }

        private Result OpenDataStorageCore(out ReferenceCountedDisposable<IStorage> storage, out Hash ncaHeaderDigest,
            ulong id, StorageId storageId)
        {
            throw new NotImplementedException();
        }

        public Result OpenDataStorageByCurrentProcess(out ReferenceCountedDisposable<IStorage> storage)
        {
            throw new NotImplementedException();
        }

        public Result OpenDataStorageByProgramId(out ReferenceCountedDisposable<IStorage> storage, ProgramId programId)
        {
            throw new NotImplementedException();
        }

        public Result OpenFileSystemWithId(out ReferenceCountedDisposable<IFileSystem> fileSystem, in FspPath path,
            ulong id, FileSystemProxyType fsType)
        {
            fileSystem = default;

            Result rc = GetProgramInfo(out ProgramInfo programInfo);
            if (rc.IsFailure()) return rc;

            AccessControl ac = programInfo.AccessControl;

            switch (fsType)
            {
                case FileSystemProxyType.Logo:
                    if (!ac.GetAccessibilityFor(AccessibilityType.MountLogo).CanRead)
                        return ResultFs.PermissionDenied.Log();
                    break;
                case FileSystemProxyType.Control:
                    if (!ac.GetAccessibilityFor(AccessibilityType.MountContentControl).CanRead)
                        return ResultFs.PermissionDenied.Log();
                    break;
                case FileSystemProxyType.Manual:
                    if (!ac.GetAccessibilityFor(AccessibilityType.MountContentManual).CanRead)
                        return ResultFs.PermissionDenied.Log();
                    break;
                case FileSystemProxyType.Meta:
                    if (!ac.GetAccessibilityFor(AccessibilityType.MountContentMeta).CanRead)
                        return ResultFs.PermissionDenied.Log();
                    break;
                case FileSystemProxyType.Data:
                    if (!ac.GetAccessibilityFor(AccessibilityType.MountContentData).CanRead)
                        return ResultFs.PermissionDenied.Log();
                    break;
                case FileSystemProxyType.Package:
                    if (!ac.GetAccessibilityFor(AccessibilityType.MountApplicationPackage).CanRead)
                        return ResultFs.PermissionDenied.Log();
                    break;
                default:
                    return ResultFs.InvalidArgument.Log();
            }

            if (fsType == FileSystemProxyType.Meta)
            {
                id = ulong.MaxValue;
            }
            else if (id == ulong.MaxValue)
            {
                return ResultFs.InvalidArgument.Log();
            }

            bool canMountSystemDataPrivate = ac.GetAccessibilityFor(AccessibilityType.MountSystemDataPrivate).CanRead;

            var normalizer = new PathNormalizer(path, GetPathNormalizerOptions(path));
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            rc = ServiceImpl.OpenFileSystem(out ReferenceCountedDisposable<IFileSystem> baseFs, out _, path, fsType,
                canMountSystemDataPrivate, id);
            if (rc.IsFailure()) return rc;

            fileSystem = baseFs;
            return Result.Success;
        }

        public Result OpenDataFileSystemByProgramId(out ReferenceCountedDisposable<IFileSystem> fileSystem,
            ProgramId programId)
        {
            throw new NotImplementedException();
        }

        public Result OpenDataStorageByDataId(out ReferenceCountedDisposable<IStorage> storage, DataId dataId,
            StorageId storageId)
        {
            throw new NotImplementedException();
        }

        public Result OpenDataFileSystemWithProgramIndex(out ReferenceCountedDisposable<IFileSystem> fileSystem,
            byte programIndex)
        {
            throw new NotImplementedException();
        }

        public Result OpenDataStorageWithProgramIndex(out ReferenceCountedDisposable<IStorage> storage,
            byte programIndex)
        {
            throw new NotImplementedException();
        }

        public Result GetRightsId(out RightsId rightsId, ProgramId programId, StorageId storageId)
        {
            throw new NotImplementedException();
        }

        public Result GetRightsIdAndKeyGenerationByPath(out RightsId rightsId, out byte keyGeneration, in FspPath path)
        {
            throw new NotImplementedException();
        }

        private Result OpenDataFileSystemCore(out ReferenceCountedDisposable<IFileSystem> fileSystem, out bool isHostFs,
            ulong id, StorageId storageId)
        {
            throw new NotImplementedException();
        }

        public Result OpenContentStorageFileSystem(out ReferenceCountedDisposable<IFileSystem> fileSystem,
            ContentStorageId contentStorageId)
        {
            fileSystem = default;

            Result rc = GetProgramInfo(out ProgramInfo programInfo);
            if (rc.IsFailure()) return rc;

            Accessibility accessibility =
                programInfo.AccessControl.GetAccessibilityFor(AccessibilityType.MountContentStorage);

            if (!accessibility.CanRead || !accessibility.CanWrite)
                return ResultFs.PermissionDenied.Log();

            rc = ServiceImpl.OpenContentStorageFileSystem(out ReferenceCountedDisposable<IFileSystem> contentFs,
                contentStorageId);
            if (rc.IsFailure()) return rc;

            fileSystem = contentFs;
            return Result.Success;
        }

        public Result RegisterExternalKey(in RightsId rightsId, in AccessKey accessKey)
        {
            Result rc = GetProgramInfo(out ProgramInfo programInfo);
            if (rc.IsFailure()) return rc;

            if (!programInfo.AccessControl.CanCall(OperationType.RegisterExternalKey))
                return ResultFs.PermissionDenied.Log();

            return ServiceImpl.RegisterExternalKey(in rightsId, in accessKey);
        }

        public Result UnregisterExternalKey(in RightsId rightsId)
        {
            Result rc = GetProgramInfo(out ProgramInfo programInfo);
            if (rc.IsFailure()) return rc;

            if (!programInfo.AccessControl.CanCall(OperationType.RegisterExternalKey))
                return ResultFs.PermissionDenied.Log();

            return ServiceImpl.UnregisterExternalKey(in rightsId);
        }

        public Result UnregisterAllExternalKey()
        {
            Result rc = GetProgramInfo(out ProgramInfo programInfo);
            if (rc.IsFailure()) return rc;

            if (!programInfo.AccessControl.CanCall(OperationType.RegisterExternalKey))
                return ResultFs.PermissionDenied.Log();

            return ServiceImpl.UnregisterAllExternalKey();
        }

        public Result RegisterUpdatePartition()
        {
            throw new NotImplementedException();
        }

        public Result OpenRegisteredUpdatePartition(out ReferenceCountedDisposable<IFileSystem> fileSystem)
        {
            throw new NotImplementedException();
        }

        public Result IsArchivedProgram(out bool isArchived, ulong processId)
        {
            throw new NotImplementedException();
        }

        public Result SetSdCardEncryptionSeed(in EncryptionSeed encryptionSeed)
        {
            Result rc = GetProgramInfo(out ProgramInfo programInfo);
            if (rc.IsFailure()) return rc;

            if (!programInfo.AccessControl.CanCall(OperationType.SetEncryptionSeed))
                return ResultFs.PermissionDenied.Log();

            return ServiceImpl.SetSdCardEncryptionSeed(in encryptionSeed);
        }

        public Result OpenSystemDataUpdateEventNotifier(out ReferenceCountedDisposable<IEventNotifier> eventNotifier)
        {
            throw new NotImplementedException();
        }

        public Result NotifySystemDataUpdateEvent()
        {
            throw new NotImplementedException();
        }

        public Result HandleResolubleAccessFailure(out bool wasDeferred, in Result nonDeferredResult)
        {
            throw new NotImplementedException();
        }

        public void IncrementRomFsRemountForDataCorruptionCount()
        {
            throw new NotImplementedException();
        }

        public void IncrementRomFsUnrecoverableDataCorruptionByRemountCount()
        {
            throw new NotImplementedException();
        }

        public void IncrementRomFsRecoveredByInvalidateCacheCount()
        {
            throw new NotImplementedException();
        }

        Result IRomFileSystemAccessFailureManager.OpenDataStorageCore(out ReferenceCountedDisposable<IStorage> storage,
            out Hash ncaHeaderDigest, ulong id, StorageId storageId)
        {
            return OpenDataStorageCore(out storage, out ncaHeaderDigest, id, storageId);
        }

        internal void SetSelfReference(ReferenceCountedDisposable<NcaFileSystemService> reference)
        {
            SelfReference = new ReferenceCountedDisposable<NcaFileSystemService>.WeakReference(reference);
        }

        private Result TryAcquireAddOnContentOpenCountSemaphore(out IUniqueLock semaphoreLock)
        {
            throw new NotImplementedException();
        }

        private Result TryAcquireRomMountCountSemaphore(out IUniqueLock semaphoreLock)
        {
            throw new NotImplementedException();
        }

        private Result GetProgramInfo(out ProgramInfo programInfo)
        {
            return ServiceImpl.GetProgramInfo(out programInfo, ProcessId);
        }

        private PathNormalizer.Option GetPathNormalizerOptions(U8Span path)
        {
            // Set the PreserveUnc flag if the path is on the host file system
            PathNormalizer.Option hostOption = IsHostFs(path) ? PathNormalizer.Option.PreserveUnc : PathNormalizer.Option.None;
            return PathNormalizer.Option.HasMountName | PathNormalizer.Option.PreserveTailSeparator | hostOption;
        }

        private bool IsHostFs(U8Span path)
        {
            int hostMountLength = StringUtils.GetLength(CommonMountNames.HostRootFileSystemMountName,
                PathTools.MountNameLengthMax);

            return StringUtils.Compare(path, CommonMountNames.HostRootFileSystemMountName, hostMountLength) == 0;
        }

        public void Dispose()
        {
            AocMountCountSemaphore?.Dispose();
            RomMountCountSemaphore?.Dispose();
        }
    }
}
