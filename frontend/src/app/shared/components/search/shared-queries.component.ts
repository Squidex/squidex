/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-output-native */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { Queries, Query } from '@app/shared/internal';
import { QueryListComponent } from './query-list.component';

@Component({
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
    /** Emits the query to search for. */
    public readonly search = output<Query | undefined>();

    /** The current query, to highlight the matching saved query. */
    public readonly queryUsed = input<Query | null>();

    /** The shared and user queries. */
    public readonly queries = input.required<Queries>();

    /** The plural name of the searched items, for example "contents". */
    public readonly types = input.required<string>();
}
