/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, Component, DestroyRef, effect, ElementRef, inject, input, viewChild } from '@angular/core';
import Cropper from 'cropperjs';
import { TooltipDirective, TranslatePipe, Types } from '@app/framework';

@Component({
    selector: 'sqx-image-editor',
    styleUrls: ['./image-cropper.component.scss'],
    templateUrl: './image-cropper.component.html',
    imports: [
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ImageCropperComponent {
    private readonly destroyRef = inject(DestroyRef);
    private cropper: Cropper | null = null;
    private data?: Cropper.Data;

    /** The URL of the image to edit. */
    public readonly imageSource = input('');

    protected readonly editor = viewChild.required<ElementRef<HTMLCanvasElement>>('editor');

    constructor() {
        const destroyRef = this.destroyRef;

        effect(() => {
            const imageSource = this.imageSource();

            this.cropper?.replace(imageSource);
        });

        afterNextRender(() => {
            this.cropper = new Cropper(this.editor().nativeElement, {
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

            this.cropper.replace(this.imageSource());
        });

        destroyRef.onDestroy(() => {
            this.cropper?.destroy();
        });
    }

    protected rotate(value: number) {
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

    protected flip(vertically: boolean) {
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

    protected reset() {
        if (this.cropper) {
            this.cropper.reset();
            this.cropper.clear();

            this.data = this.cropper.getData();
        }
    }

    public async toFile(): Promise<File | null> {
        return new Promise<File | null>(resolve => {
            if (!this.cropper) {
                return resolve(null);
            } else {
                const data = this.cropper.getData();

                if (Types.equals(data, this.data)) {
                    resolve(null);
                } else {
                    this.data = data;

                    this.cropper.getCroppedCanvas().toBlob(blob => {
                        if (blob) {
                            const file = new File([blob], 'image.png', { type: 'image.png' });

                            resolve(file);
                        } else {
                            resolve(null);
                        }
                    }, 'image.png');
                }
            }

            return undefined;
        });
    }
}
