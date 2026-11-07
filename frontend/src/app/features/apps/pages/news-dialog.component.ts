/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input, output } from '@angular/core';
import { FeatureDto, HelpMarkdownPipe, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-news-dialog',
    styleUrls: ['./news-dialog.component.scss'],
    templateUrl: './news-dialog.component.html',
    imports: [
        HelpMarkdownPipe,
        ModalDialogComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class NewsDialogComponent {
    /** Emits when the user closes the dialog. */
    public readonly dialogClose = output();

    /** The new features to present to the user. */
    public readonly features = input.required<ReadonlyArray<FeatureDto>>();
}
