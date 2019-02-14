/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';

import { FeatureDto } from '@app/shared';

@Component({
    selector: 'sqx-news-dialog',
    styleUrls: ['./news-dialog.component.scss'],
    templateUrl: './news-dialog.component.html'
})
export class NewsDialogComponent {
    @Input()
    public features: FeatureDto[];

    @Output()
    public closed = new EventEmitter();

    public close() {
        this.closed.emit();
    }
}