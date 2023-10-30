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
    templateUrl: './news-dialog.component.html',
})
export class NewsDialogComponent {
    @Output()
    public dialogClose = new EventEmitter();

    @Input({ required: true })
    public features!: ReadonlyArray<FeatureDto>;

    public trackByFeature(_index: number, feature: FeatureDto) {
        return feature;
    }
}
