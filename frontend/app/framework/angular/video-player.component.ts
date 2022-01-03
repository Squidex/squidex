/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, OnDestroy, Renderer2, ViewChild } from '@angular/core';
import { ResourceLoaderService } from '@app/framework/internal';

declare const videojs: any;

@Component({
    selector: 'sqx-video-player',
    styleUrls: ['./video-player.component.scss'],
    templateUrl: './video-player.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VideoPlayerComponent implements AfterViewInit, OnDestroy, OnChanges {
    private player: any;

    @Input()
    public source = '';

    @Input()
    public mimeType = '';

    @ViewChild('video', { static: false })
    public video!: ElementRef<HTMLVideoElement>;

    constructor(
        private readonly resourceLoader: ResourceLoaderService,
        private readonly renderer: Renderer2,
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

    public ngAfterViewInit() {
        Promise.all([
            this.resourceLoader.loadLocalScript('dependencies/videojs/video.min.js'),
            this.resourceLoader.loadLocalStyle('dependencies/videojs/video-js.min.css'),
        ]).then(() => {
            this.renderer.removeClass(this.video.nativeElement, 'hidden');

            this.player = videojs(this.video.nativeElement, {
                fluid: true,
            });

            this.ngOnChanges();
        });
    }
}
