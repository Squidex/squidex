/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { ConfirmClickDirective, IndexDto, IndexesState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-index',
    styleUrls: ['./index.component.scss'],
    templateUrl: './index.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        TranslatePipe,
    ],
})
export class IndexComponent {
    private readonly indexesState = inject(IndexesState);

    /** The index to show. */
    public readonly index = input.required<IndexDto>();

    protected readonly isExpanded = signal(false);

    protected toggleExpanded() {
        this.isExpanded.update(x => !x);
    }

    protected delete() {
        this.indexesState.delete(this.index());
    }
}
