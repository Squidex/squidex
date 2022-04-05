/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, OnDestroy, ViewChild } from '@angular/core';
import { FocusedImage, FocusPicker } from 'image-focus';
import { Types } from '@app/framework';
import { AnnotateAssetDto, AssetDto } from '@app/shared/services/assets.service';

@Component({
    selector: 'sqx-image-focus-point',
    styleUrls: ['./image-focus-point.component.scss'],
    templateUrl: './image-focus-point.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImageFocusPointComponent implements AfterViewInit, OnDestroy, OnChanges {
    private readonly previewImages: FocusedImage[] = [];
    private focusPicker: FocusPicker | null = null;
    private x = 0;
    private y = 0;

    @Input()
    public imageSource = '';

    @Input()
    public focusPoint: any;

    @ViewChild('image', { static: false })
    public image!: ElementRef<HTMLImageElement>;

    @ViewChild('previewWide', { static: false })
    public previewWide!: ElementRef<HTMLImageElement>;

    @ViewChild('previewSmall', { static: false })
    public previewSmall!: ElementRef<HTMLImageElement>;

    @ViewChild('previewNormal', { static: false })
    public previewNormal!: ElementRef<HTMLImageElement>;

    public ngOnDestroy() {
        if (this.focusPicker) {
            this.focusPicker.stopListening();
        }
    }

    public ngOnChanges() {
        const { x, y } = getFocusPoint(this.focusPoint);

        this.x = x;
        this.y = y;
    }

    public ngAfterViewInit() {
        const focus = { x: this.x, y: this.y };

        const properties = { focus, debounceTime: 50 };

        this.previewImages.push(new FocusedImage(this.previewWide.nativeElement, properties));
        this.previewImages.push(new FocusedImage(this.previewSmall.nativeElement, properties));
        this.previewImages.push(new FocusedImage(this.previewNormal.nativeElement, properties));

        this.focusPicker = new FocusPicker(this.image.nativeElement, {
            focus,
            onChange: newFocus => {
                this.x = newFocus.x;
                this.y = newFocus.y;

                for (const preview of this.previewImages) {
                    preview.setFocus(newFocus);
                }
            },
        });
    }

    public submit(asset: AssetDto): AnnotateAssetDto | null {
        const previous = getFocusPoint(asset.metadata);

        if (previous.x === this.x && previous.y === this.y) {
            return null;
        }

        const metadata = { ...asset.metadata, focusX: this.x, focusY: this.y };

        return { metadata };
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
