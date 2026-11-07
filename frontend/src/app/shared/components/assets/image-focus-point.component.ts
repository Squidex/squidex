/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, ChangeDetectionStrategy, Component, DestroyRef, ElementRef, inject, input, linkedSignal, viewChild } from '@angular/core';
import { FocusedImage, FocusPicker } from 'image-focus';
import { TranslatePipe, Types } from '@app/framework';
import { AnnotateAssetDto, AssetDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-image-focus-point',
    styleUrls: ['./image-focus-point.component.scss'],
    templateUrl: './image-focus-point.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class ImageFocusPointComponent {
    private readonly image = viewChild.required<ElementRef<HTMLImageElement>>('image');
    private readonly previewWide = viewChild.required<ElementRef<HTMLImageElement>>('previewWide');
    private readonly previewSmall = viewChild.required<ElementRef<HTMLImageElement>>('previewSmall');
    private readonly previewNormal = viewChild.required<ElementRef<HTMLImageElement>>('previewNormal');
    private readonly focus = linkedSignal(() => getFocusPoint(this.focusPoint()));

    /** The URL of the image. */
    public readonly imageSource = input('');

    /** The asset metadata with the initial focus point (focusX and focusY). */
    public readonly focusPoint = input<any>();

    constructor() {
        const destroyRef = inject(DestroyRef);

        afterNextRender(() => {
            const properties = { focus: this.focus(), debounceTime: 50 };

            const previewImages = [
                new FocusedImage(this.previewWide().nativeElement, properties),
                new FocusedImage(this.previewSmall().nativeElement, properties),
                new FocusedImage(this.previewNormal().nativeElement, properties),
            ];

            const focusPicker = new FocusPicker(this.image().nativeElement, {
                focus: properties.focus,
                onChange: newFocus => {
                    this.focus.set({ x: newFocus.x, y: newFocus.y });

                    for (const preview of previewImages) {
                        preview.setFocus(newFocus);
                    }
                },
            });

            destroyRef.onDestroy(() => {
                focusPicker.stopListening();
            });
        });
    }

    public getChanges(asset: AssetDto): AnnotateAssetDto | null {
        const previous = getFocusPoint(asset.metadata);

        const focus = this.focus();
        if (previous.x === focus.x && previous.y === focus.y) {
            return null;
        }

        return new AnnotateAssetDto({
            metadata: {
                ...asset.metadata,
                focusX: focus.x,
                focusY: focus.y,
            },
        });
    }
}

function getFocusPoint(value: any): { x: number; y: number } {
    let x = 0;
    let y = 0;

    if (value && Types.isNumber(value.focusX)) {
        x = value.focusX;
    }

    if (value && Types.isNumber(value.focusY)) {
        y = value.focusY;
    }

    return { x, y };
}
