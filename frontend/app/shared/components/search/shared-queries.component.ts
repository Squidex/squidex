/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Queries, Query } from '@app/shared/internal';

@Component({
    selector: 'sqx-shared-queries[queries][types]',
    styleUrls: ['./shared-queries.component.scss'],
    templateUrl: './shared-queries.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavedQueriesComponent {
    @Output()
    public search = new EventEmitter<Query>();

    @Input()
    public queryUsed: Query | undefined | null;

    @Input()
    public queries!: Queries;

    @Input()
    public types = '';
}
