/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { ConfirmClickDirective, IndexDto, IndexesState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-index',
    styleUrls: ['./index.component.scss'],
    templateUrl: './index.component.html',
    imports: [
        ConfirmClickDirective,
        TranslatePipe,
    ],
})
export class IndexComponent {
    @Input({ required: true })
    public index!: IndexDto;

    public isExpanded?: boolean | null;

    constructor(
        private readonly indexesState: IndexesState,
    ) {
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }

    public delete() {
        this.indexesState.delete(this.index);
    }
}
