// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FFMpegCore;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class FFMpegAssetMetadataSource : IAssetMetadataSource
{
    public async Task EnhanceAsync(UploadAssetCommand command,
        CancellationToken ct)
    {
        if (command.Type != AssetType.Unknown)
        {
            return;
        }

        try
        {
            var analysis = await FFProbe.AnalyseAsync(command.File.OpenRead(), cancellationToken: ct);

            void TryAddInt(string name, long? value)
            {
                if (value > 0)
                {
                    command.Metadata[name] = value.Value;
                }
            }

            void TryAddTimeSpan(string name, TimeSpan value)
            {
                if (value != TimeSpan.Zero)
                {
                    command.Metadata[name] = value.ToString();
                }
            }

            var audioStream = analysis.AudioStreams.FirstOrDefault();
            if (audioStream != null)
            {
                TryAddTimeSpan(KnownMetadataKeys.Duration, audioStream.Duration);

                TryAddInt(KnownMetadataKeys.AudioBitrate, audioStream.BitRate / 1000);
                TryAddInt(KnownMetadataKeys.AudioSampleRate, audioStream.SampleRateHz);

                command.Type = AssetType.Audio;
            }

            var videoStream = analysis.VideoStreams.FirstOrDefault();
            if (videoStream != null)
            {
                TryAddTimeSpan(KnownMetadataKeys.Duration, videoStream.Duration);

                TryAddInt(KnownMetadataKeys.VideoWidth, videoStream.Width);
                TryAddInt(KnownMetadataKeys.VideoHeight, videoStream.Height);

                command.Type = AssetType.Video;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IEnumerable<string> Format(Asset asset)
    {
        yield break;
    }
}
