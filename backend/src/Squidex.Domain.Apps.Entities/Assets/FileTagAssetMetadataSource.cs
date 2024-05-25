// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using TagLib;
using TagLib.Image;
using static TagLib.File;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class FileTagAssetMetadataSource : IAssetMetadataSource
{
    private sealed class FileAbstraction : IFileAbstraction
    {
        private readonly IAssetFile file;

        public string Name
        {
            get => file.FileName;
        }

        public Stream ReadStream
        {
            get => file.OpenRead();
        }

        public Stream WriteStream
        {
            get => throw new NotSupportedException();
        }

        public FileAbstraction(IAssetFile file)
        {
            this.file = file;
        }

        public void CloseStream(Stream stream)
        {
            stream.Close();
        }
    }

    public Task EnhanceAsync(UploadAssetCommand command,
        CancellationToken ct)
    {
        try
        {
            using (var file = Create(new FileAbstraction(command.File), ReadStyle.Average))
            {
                if (file.Properties == null)
                {
                    return Task.CompletedTask;
                }

                var type = file.Properties.MediaTypes;

                if (type == MediaTypes.Audio)
                {
                    command.Type = AssetType.Audio;
                }
                else if (type == MediaTypes.Photo)
                {
                    command.Type = AssetType.Image;
                }
                else if (type.HasFlag(MediaTypes.Video))
                {
                    command.Type = AssetType.Video;
                }

                var pw = file.Properties.PhotoWidth;
                var ph = file.Properties.PhotoHeight;

                if (pw > 0 && ph > 0)
                {
                    command.Metadata[KnownMetadataKeys.PixelWidth] = pw;
                    command.Metadata[KnownMetadataKeys.PixelHeight] = ph;
                }

                void TryAddString(string name, string? value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        command.Metadata.Add(name, value);
                    }
                }

                void TryAddInt(string name, int? value)
                {
                    if (value > 0)
                    {
                        command.Metadata.Add(name, (double)value.Value);
                    }
                }

                void TryAddDouble(string name, double? value)
                {
                    if (value > 0)
                    {
                        command.Metadata.Add(name, value.Value);
                    }
                }

                void TryAddTimeSpan(string name, TimeSpan value)
                {
                    if (value != TimeSpan.Zero)
                    {
                        command.Metadata.Add(name, value.ToString());
                    }
                }

                if (file.Tag is ImageTag imageTag)
                {
                    TryAddDouble(KnownMetadataKeys.Latitude, imageTag.Latitude);
                    TryAddDouble(KnownMetadataKeys.Longitude, imageTag.Longitude);

                    TryAddString(KnownMetadataKeys.Created, imageTag.DateTime?.ToIso8601());
                }

                TryAddTimeSpan(KnownMetadataKeys.Duration, file.Properties.Duration);

                TryAddInt(KnownMetadataKeys.BitsPerSample, file.Properties.BitsPerSample);
                TryAddInt(KnownMetadataKeys.AudioBitrate, file.Properties.AudioBitrate);
                TryAddInt(KnownMetadataKeys.AudioChannels, file.Properties.AudioChannels);
                TryAddInt(KnownMetadataKeys.AudioSampleRate, file.Properties.AudioSampleRate);
                TryAddInt(KnownMetadataKeys.ImageQuality, file.Properties.PhotoQuality);

                TryAddInt(KnownMetadataKeys.VideoWidth, file.Properties.VideoWidth);
                TryAddInt(KnownMetadataKeys.VideoHeight, file.Properties.VideoHeight);

                TryAddString(KnownMetadataKeys.Description, file.Properties.Description);
            }

            return Task.CompletedTask;
        }
        catch
        {
            return Task.CompletedTask;
        }
    }

    public IEnumerable<string> Format(Asset asset)
    {
        if (asset.Type == AssetType.Video)
        {
            if (asset.Metadata.TryGetNumber(KnownMetadataKeys.VideoWidth, out var w) &&
                asset.Metadata.TryGetNumber(KnownMetadataKeys.VideoHeight, out var h))
            {
                yield return $"{w}x{h}pt";
            }

            if (asset.Metadata.TryGetString(KnownMetadataKeys.Duration, out var duration))
            {
                yield return duration;
            }
        }
        else if (asset.Type == AssetType.Audio)
        {
            if (asset.Metadata.TryGetString(KnownMetadataKeys.Duration, out var duration))
            {
                yield return duration;
            }
        }
    }
}
