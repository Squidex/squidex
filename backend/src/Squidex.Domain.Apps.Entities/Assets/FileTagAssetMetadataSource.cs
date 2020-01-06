﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Tasks;
using TagLib;
using TagLib.Image;
using static TagLib.File;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class FileTagAssetMetadataSource : IAssetMetadataSource
    {
        private sealed class FileAbstraction : IFileAbstraction
        {
            private readonly AssetFile file;

            public string Name
            {
                get { return file.FileName; }
            }

            public Stream ReadStream
            {
                get { return file.OpenRead(); }
            }

            public Stream WriteStream
            {
                get { throw new NotSupportedException(); }
            }

            public FileAbstraction(AssetFile file)
            {
                this.file = file;
            }

            public void CloseStream(Stream stream)
            {
                stream.Close();
            }
        }

        public Task EnhanceAsync(UploadAssetCommand command, HashSet<string>? tags)
        {
            Enhance(command, tags);

            return TaskHelper.Done;
        }

        private void Enhance(UploadAssetCommand command, HashSet<string>? tags)
        {
            try
            {
                using (var file = Create(new FileAbstraction(command.File), ReadStyle.Average))
                {
                    if (file.Properties == null)
                    {
                        return;
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

                    if (pw > 0 && pw > 0)
                    {
                        command.Metadata.SetPixelWidth(pw);
                        command.Metadata.SetPixelHeight(ph);

                        if (tags != null)
                        {
                            tags.Add("image");

                            var wh = pw + ph;

                            if (wh > 2000)
                            {
                                tags.Add("image/large");
                            }
                            else if (wh > 1000)
                            {
                                tags.Add("image/medium");
                            }
                            else
                            {
                                tags.Add("image/small");
                            }
                        }
                    }

                    void TryAddString(string name, string? value)
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            command.Metadata.Add(name, JsonValue.Create(value));
                        }
                    }

                    void TryAddInt(string name, int? value)
                    {
                        if (value > 0)
                        {
                            command.Metadata.Add(name, JsonValue.Create(value));
                        }
                    }

                    void TryAddDouble(string name, double? value)
                    {
                        if (value > 0)
                        {
                            command.Metadata.Add(name, JsonValue.Create(value));
                        }
                    }

                    void TryAddTimeSpan(string name, TimeSpan value)
                    {
                        if (value != TimeSpan.Zero)
                        {
                            command.Metadata.Add(name, JsonValue.Create(value.ToString()));
                        }
                    }

                    if (file.Tag is ImageTag imageTag)
                    {
                        TryAddDouble("locationLatitude", imageTag.Latitude);
                        TryAddDouble("locationLongitude", imageTag.Longitude);

                        TryAddString("created", imageTag.DateTime?.ToIso8601());
                    }

                    TryAddTimeSpan("duration", file.Properties.Duration);

                    TryAddInt("audioBitrate", file.Properties.AudioBitrate);
                    TryAddInt("audioChannels", file.Properties.AudioChannels);
                    TryAddInt("audioSampleRate", file.Properties.AudioSampleRate);
                    TryAddInt("bitsPerSample", file.Properties.BitsPerSample);
                    TryAddInt("imageQuality", file.Properties.PhotoQuality);
                    TryAddInt("videoWidth", file.Properties.VideoWidth);
                    TryAddInt("videoHeight", file.Properties.VideoHeight);

                    TryAddString("description", file.Properties.Description);
                }
            }
            catch
            {
                return;
            }
        }

        public IEnumerable<string> Format(IAssetEntity asset)
        {
            var metadata = asset.Metadata;

            switch (asset.Type)
            {
                case AssetType.Image:
                    {
                        if (metadata.TryGetNumber("pixelWidth", out var w) &&
                            metadata.TryGetNumber("pixelHeight", out var h))
                        {
                            yield return $"{w}x{h}px";
                        }

                        break;
                    }

                case AssetType.Video:
                    {
                        if (metadata.TryGetNumber("videoWidth", out var w) &&
                            metadata.TryGetNumber("videoHeight", out var h))
                        {
                            yield return $"{w}x{h}pt";
                        }

                        if (metadata.TryGetString("duration", out var duration))
                        {
                            yield return duration;
                        }

                        break;
                    }

                case AssetType.Audio:
                    {
                        if (metadata.TryGetString("duration", out var duration))
                        {
                            yield return duration;
                        }

                        break;
                    }
            }
        }
    }
}
