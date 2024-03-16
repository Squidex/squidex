/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, OnDestroy, ViewChild } from '@angular/core';
import Cropper from 'cropperjs';
import { Types } from '@app/framework';

@Component({
    selector: 'sqx-image-editor',
    styleUrls: ['./image-cropper.component.scss'],
    templateUrl: './image-cropper.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImageCropperComponent implements AfterViewInit, OnDestroy, OnChanges {
    private cropper: Cropper | null = null;
    private data?: Cropper.Data;

    @Input()
    public imageSource = '';

    @ViewChild('editor', { static: false })
    public editor!: ElementRef<HTMLCanvasElement>;

    public ngOnDestroy() {
        if (this.cropper) {
            this.cropper.destroy();
        }
    }

    public ngOnChanges() {
        if (this.cropper) {
            this.cropper.replace(this.imageSource);
        }
    }

    public ngAfterViewInit() {
        this.cropper = new Cropper(this.editor.nativeElement, {
            ready: () => {
                if (this.cropper) {
                    this.data = this.cropper.getData();
                }
            },
            autoCrop: false,
            background: false,
            minContainerHeight: 0,
            minContainerWidth: 0,
            movable: false,
            zoomOnTouch: false,
            zoomOnWheel: false,
            viewMode: 0,
        });

        this.cropper.replace(this.imageSource);
    }

    public rotate(value: number) {
        if (this.cropper) {
            this.cropper.rotate(value);

            const canvasData = this.cropper.getCanvasData();
            const containerData = this.cropper.getContainerData();

            const dx = containerData.width / canvasData.naturalWidth;
            const dy = containerData.height / canvasData.naturalHeight;

            this.cropper.zoomTo(Math.min(dx, dy), {
                x: containerData.width / 2,
                y: containerData.height / 2,
            });
        }
    }

    public flip(vertically: boolean) {
        if (this.cropper) {
            const { rotate, scaleX, scaleY } = this.cropper.getData();

            if (rotate === 90 || rotate === 270) {
                vertically = !vertically;
            }

            if (vertically) {
                this.cropper.scale(scaleX, -1 * scaleY);
            } else {
                this.cropper.scale(-1 * scaleX, scaleY);
            }
        }
    }

    public zoomIn() {
        if (this.cropper) {
            this.cropper.zoom(0.1);
        }
    }

    public zoomOut() {
        if (this.cropper) {
            this.cropper.zoom(-0.1);
        }
    }

    public reset() {
        if (this.cropper) {
            this.cropper.reset();
            this.cropper.clear();

            this.data = this.cropper.getData();
        }
    }

    public toFile(): Promise<Blob | null> {
        return new Promise<Blob | null>(resolve => {
            if (!this.cropper) {
                return resolve(null);
            } else {
                const data = this.cropper.getData();

                if (Types.equals(data, this.data)) {
                    resolve(null);
                } else {
                    this.data = data;

                    this.cropper.getCroppedCanvas().toBlob(blob => {
                        resolve(blob);
                    });
                }
            }

            return undefined;
        });
    }
}
