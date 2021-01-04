/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, OnDestroy, Renderer2, ViewChild } from '@angular/core';
import { ResourceLoaderService } from '@app/framework/internal';

declare var videojs: any;

@Component({
    selector: 'sqx-video-player',
    styleUrls: ['./video-player.component.scss'],
    templateUrl: './video-player.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class VideoPlayerComponent implements AfterViewInit, OnDestroy, OnChanges {
    private player: any;

    @Input()
    public source: string;

    @Input()
    public mimeType: string;

    @ViewChild('video', { static: false })
    public video: ElementRef<HTMLVideoElement>;

    constructor(
        private readonly resourceLoader: ResourceLoaderService,
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnDestroy() {
        this.player?.dispose();
    }

    public ngOnChanges() {
        if (this.player) {
            if (this.source) {
                this.player.src({ type: this.mimeType, src: this.source });
            } else {
                this.player.src();
            }
        }
    }

    public ngAfterViewInit(): void {
        Promise.all([
            this.resourceLoader.loadScript('https://vjs.zencdn.net/7.10.2/video.min.js'),
            this.resourceLoader.loadStyle('https://vjs.zencdn.net/7.10.2/video-js.css')
        ]).then(() => {
            this.player = videojs(this.video.nativeElement, {
                fluid: true
            });

            this.renderer.removeClass(this.video.nativeElement, 'hidden');

            this.ngOnChanges();
        });
    }
}