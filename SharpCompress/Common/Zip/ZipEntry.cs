﻿using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common.Zip.Headers;

namespace SharpCompress.Common.Zip
{
    public class ZipEntry : Entry
    {
        private readonly bool directory;
        private readonly ZipFilePart filePart;
        private readonly DateTime? lastModifiedTime;

        internal ZipEntry(ZipFilePart filePart)
        {
            this.filePart = filePart;
            directory = filePart.Header.Name.EndsWith("/");
            lastModifiedTime = Utility.DosDateToDateTime(filePart.Header.LastModifiedDate,
                                                         filePart.Header.LastModifiedTime);
        }

        public override uint Crc
        {
            get { return filePart.Header.Crc; }
        }

        public override string FilePath
        {
            get { return filePart.Header.Name; }
        }

        public override long CompressedSize
        {
            get { return filePart.Header.CompressedSize; }
        }

        public override long Size
        {
            get { return filePart.Header.UncompressedSize; }
        }

        public override DateTime? LastModifiedTime
        {
            get { return lastModifiedTime; }
        }

        public override DateTime? CreatedTime
        {
            get { return null; }
        }

        public override DateTime? LastAccessedTime
        {
            get { return null; }
        }

        public override DateTime? ArchivedTime
        {
            get { return null; }
        }

        public override bool IsEncrypted
        {
            get { return false; }
        }

        public override bool IsDirectory
        {
            get { return directory; }
        }

        public override bool IsSplit
        {
            get { return false; }
        }

        internal override IEnumerable<FilePart> Parts
        {
            get { return filePart.AsEnumerable<FilePart>(); }
        }

        internal static IEnumerable<ZipEntry> GetEntries(Stream stream)
        {
            foreach (ZipHeader h in ZipHeaderFactory.ReadHeaderNonseekable(stream))
            {
                if (h != null)
                {
                    switch (h.ZipHeaderType)
                    {
                        case ZipHeaderType.LocalEntry:
                            {
                                yield return new ZipEntry(new ZipFilePart(h as LocalEntryHeader));
                            }
                            break;
                        case ZipHeaderType.DirectoryEnd:
                            {
                                yield break;
                            }
                    }
                }
            }
        }
    }
}