﻿using System.Collections.Generic;
using NUnrar.Common;
using NUnrar.Headers;
using NUnrar.IO;

namespace NUnrar.Rar
{
    /// <summary>
    /// A RarArchiveVolume is a single rar file that may or may not be a split RarArchive.  A Rar Archive is one to many Rar Parts
    /// </summary>
    public abstract class RarVolume : Volume
    {
        private RarHeaderFactory headerFactory;
        private ReaderOptions options;

        internal RarVolume(StreamingMode mode, ReaderOptions options)
        {
            this.options = options;
            headerFactory = new RarHeaderFactory(mode, options);
        }

        internal StreamingMode Mode
        {
            get
            {
                return headerFactory.StreamingMode;
            }
        }

        internal abstract IEnumerable<RarFilePart> ReadFileParts();

        internal abstract RarFilePart CreateFilePart(FileHeader fileHeader, MarkHeader markHeader);

        internal IEnumerable<RarFilePart> GetVolumeFileParts()
        {
            MarkHeader previousMarkHeader = null;
            foreach (RarHeader header in headerFactory.ReadHeaders(this.Stream))
            {
                switch (header.HeaderType)
                {
                    case HeaderType.ArchiveHeader:
                        {
                            ArchiveHeader = header as ArchiveHeader;
                        }
                        break;
                    case HeaderType.MarkHeader:
                        {
                            previousMarkHeader = header as MarkHeader;
                        }
                        break;
                    case HeaderType.FileHeader:
                        {
                            FileHeader fh = header as FileHeader;

                            if (!fh.FileFlags.HasFlag(FileFlags.DIRECTORY))
                            {
                                RarFilePart fp = CreateFilePart(fh, previousMarkHeader);
                                yield return fp;
                            }
                            else if (options.HasFlag(ReaderOptions.GiveDirectoryEntries))
                            {
                                RarFilePart fp = CreateFilePart(fh, previousMarkHeader);
                                yield return fp;
                            }
                        }
                        break;
                }
            }
        }

        internal ArchiveHeader ArchiveHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// RarArchive is the first volume of a multi-part archive.
        /// Only Rar 3.0 format and higher
        /// </summary>
        public override bool IsFirstVolume
        {
            get
            {
                return ArchiveHeader.ArchiveHeaderFlags.HasFlag(ArchiveFlags.FIRSTVOLUME);
            }
        }

        /// <summary>
        /// RarArchive is part of a multi-part archive.
        /// </summary>
        public override bool IsMultiVolume
        {
            get
            {
                return ArchiveHeader.ArchiveHeaderFlags.HasFlag(ArchiveFlags.VOLUME);
            }
        }

        /// <summary>
        /// RarArchive is SOLID (this means the Archive saved bytes by reusing information which helps for archives containing many small files).
        /// Currently, NUnrar cannot decompress SOLID archives.
        /// </summary>
        public bool IsSolidArchive
        {
            get
            {
                return ArchiveHeader.ArchiveHeaderFlags.HasFlag(ArchiveFlags.SOLID);
            }
        }
    }
}
