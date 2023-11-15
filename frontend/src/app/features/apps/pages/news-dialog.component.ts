/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FeatureDto, HelpMarkdownPipe, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-news-dialog',
    styleUrls: ['./news-dialog.component.scss'],
    templateUrl: './news-dialog.component.html',
    imports: [
        HelpMarkdownPipe,
        ModalDialogComponent,
        NgFor,
        TooltipDirective,
        TranslatePipe,
    ],
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
