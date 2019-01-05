﻿using System;
using System.IO;

namespace LibHac.IO
{
    public class LocalFileSystem : IFileSystem
    {
        private string BasePath { get; }

        public LocalFileSystem(string basePath)
        {
            BasePath = Path.GetFullPath(basePath);
        }

        internal string ResolveLocalPath(string path)
        {
            return Path.Combine(BasePath, path.TrimStart('/'));
        }

        public void CreateDirectory(string path)
        {
            path = PathTools.Normalize(path);
            Directory.CreateDirectory(ResolveLocalPath(path));
        }

        public void CreateFile(string path, long size)
        {
            path = PathTools.Normalize(path);
            string localPath = ResolveLocalPath(path);
            string localDir = Path.GetDirectoryName(localPath);

            if (localDir != null) Directory.CreateDirectory(localDir);

            using (FileStream stream = File.Create(localPath))
            {
                stream.SetLength(size);
            }
        }

        public void DeleteDirectory(string path)
        {
            path = PathTools.Normalize(path);

            string resolveLocalPath = ResolveLocalPath(path);
            Directory.Delete(resolveLocalPath);
        }

        public void DeleteFile(string path)
        {
            path = PathTools.Normalize(path);

            string resolveLocalPath = ResolveLocalPath(path);
            File.Delete(resolveLocalPath);
        }

        public IDirectory OpenDirectory(string path, OpenDirectoryMode mode)
        {
            path = PathTools.Normalize(path);

            return new LocalDirectory(this, path, mode);
        }

        public IFile OpenFile(string path, OpenMode mode)
        {
            path = PathTools.Normalize(path);

            string localPath = ResolveLocalPath(path);
            return new LocalFile(localPath, mode);
        }

        public void RenameDirectory(string srcPath, string dstPath)
        {
            srcPath = PathTools.Normalize(srcPath);
            dstPath = PathTools.Normalize(dstPath);

            string srcLocalPath = ResolveLocalPath(srcPath);
            string dstLocalPath = ResolveLocalPath(dstPath);

            string directoryName = Path.GetDirectoryName(dstLocalPath);
            if (directoryName != null) Directory.CreateDirectory(directoryName);
            Directory.Move(srcLocalPath, dstLocalPath);
        }

        public void RenameFile(string srcPath, string dstPath)
        {
            srcPath = PathTools.Normalize(srcPath);
            dstPath = PathTools.Normalize(dstPath);

            string srcLocalPath = ResolveLocalPath(srcPath);
            string dstLocalPath = ResolveLocalPath(dstPath);
            string dstLocalDir = Path.GetDirectoryName(dstLocalPath);

            if (dstLocalDir != null) Directory.CreateDirectory(dstLocalDir);
            File.Move(srcLocalPath, dstLocalPath);
        }

        public bool DirectoryExists(string path)
        {
            path = PathTools.Normalize(path);

            return Directory.Exists(ResolveLocalPath(path));
        }

        public bool FileExists(string path)
        {
            path = PathTools.Normalize(path);

            return File.Exists(ResolveLocalPath(path));
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }
    }
}