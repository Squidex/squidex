/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-output-native */

import { booleanAttribute, Component, input, output } from '@angular/core';
import { StopClickDirective, TranslatePipe } from '@app/framework';
import { equalsQuery, Query, SavedQuery } from '@app/shared/internal';

@Component({
    selector: 'sqx-query-list',
    styleUrls: ['./query-list.component.scss'],
    templateUrl: './query-list.component.html',
    imports: [
        StopClickDirective,
        TranslatePipe,
    ],
})
export class QueryListComponent {
    /** Emits the query to search for. */
    public readonly search = output<Query | undefined>();

    /** Emits the saved query to remove. */
    public readonly remove = output<SavedQuery>();

    /** The current query, to highlight the matching saved query. */
    public readonly queryUsed = input<Query | null>();

    /** The saved queries to render. */
    public readonly queries = input<ReadonlyArray<SavedQuery> | null>();

    /** Shows a button to remove a query and a hint when there are no queries. */
    public readonly canRemove = input(false, { transform: booleanAttribute });

    /** The plural name of the searched items, for example "contents". */
    public readonly types = input('');

    protected isSelectedQuery(saved: SavedQuery) {
        return equalsQuery(saved.query, this.queryUsed());
    }
}
