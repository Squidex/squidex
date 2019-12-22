/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';

import { picasso } from '@app/framework/internal';

@Component({
    selector: 'sqx-avatar',
    template: `
        <img *ngIf="imageSource"
            [style.width]="sizeInPx"
            [style.height]="sizeInPx"
            [src]="imageSource | sqxSafeUrl"
        />
    `
})
export class AvatarComponent implements OnChanges {
    @Input()
    public identifier: string;

    @Input()
    public image: string;

    @Input()
    public size = 50;

    public imageSource: string | null;
    public sizeInPx: string;

    public ngOnChanges() {
        this.imageSource = this.image || this.createSvg();

        this.sizeInPx = `${this.size}px`;
    }

    private createSvg() {
        if (!this.identifier) {
            return null;
        }

        const svg = picasso(this.identifier);

        return `data:image/svg+xml;utf8,${svg}`;
    }
}