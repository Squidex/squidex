/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, Component, DestroyRef, effect, ElementRef, inject, input, Renderer2, signal, viewChild } from '@angular/core';
import { ResourceLoaderService } from '@app/framework/internal';

declare const videojs: any;

@Component({
    selector: 'sqx-video-player',
    styleUrls: ['./video-player.component.scss'],
    templateUrl: './video-player.component.html',
})
export class VideoPlayerComponent {
    private readonly destroyRef = inject(DestroyRef);
    private readonly renderer = inject(Renderer2);
    private readonly resourceLoader = inject(ResourceLoaderService);

    /** The URL of the video. */
    public readonly source = input('');

    /** The mime type of the video, for example "video/mp4". */
    public readonly mimeType = input('');

    protected readonly player = signal<any>(null);

    protected readonly video = viewChild.required<ElementRef<HTMLVideoElement>>('video');

    constructor() {
        afterNextRender(async () => {
            await Promise.all([
                this.resourceLoader.loadLocalScript('dependencies/videojs/video.min.js'),
                this.resourceLoader.loadLocalStyle('dependencies/videojs/videojs.min.css'),
            ]);

            const video = this.video().nativeElement;

            this.renderer.removeClass(video, 'hidden');
            this.player.set(videojs(video, { fluid: true }));
        });

        effect(() => {
            const player = this.player();
            if (!player) {
                return;
            }

            const source = this.source();
            if (source) {
                player.src({ type: this.mimeType(), src: source });
            } else {
                player.src();
            }
        });

        this.destroyRef.onDestroy(() => {
            this.player()?.dispose();
        });
    }
}
