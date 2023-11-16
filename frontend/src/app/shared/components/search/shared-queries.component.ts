/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { Queries, Query } from '@app/shared/internal';
import { QueryListComponent } from './query-list.component';

@Component({
    standalone: true,
    selector: 'sqx-shared-queries',
    styleUrls: ['./shared-queries.component.scss'],
    templateUrl: './shared-queries.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        QueryListComponent,
        TranslatePipe,
    ],
})
export class SavedQueriesComponent {
    @Output()
    public search = new EventEmitter<Query>();

    @Input()
    public queryUsed: Query | undefined | null;

    @Input({ required: true })
    public queries!: Queries;

    @Input({ required: true })
    public types = '';
}
