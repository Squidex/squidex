/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { compareStringsAsc, Types } from '@app/framework';

import { UIState } from './ui.state';

import { encodeQuery, Query } from './query';

export interface SavedQuery {
    // The name of the query.
    name: string;

    // The deserialized value.
    query?: Query;

    // The raw value of the query.
    queryJson?: string;
}

const OLDEST_FIRST: Query = {
    sort: [
        { path: 'lastModified', order: 'descending' }
    ]
};

export class Queries {
    public queries: Observable<SavedQuery[]>;

    public defaultQueries: SavedQuery[] = [
        { name: 'All (newest first)', queryJson: '' },
        { name: 'All (oldest first)', queryJson: encodeQuery(OLDEST_FIRST), query: OLDEST_FIRST }
    ];

    constructor(
        private readonly uiState: UIState,
        private readonly prefix: string
    ) {
        this.queries = this.uiState.get(`${this.prefix}.queries`, {}).pipe(
            map(settings => {
                let queries = Object.keys(settings).map(name => parseStored(name, settings[name]));

                return queries.sort((a, b) => compareStringsAsc(a.name, b.name));
            })
        );
    }

    public add(key: string, query: Query) {
        this.uiState.set(`${this.prefix}.queries.${key}`, JSON.stringify(query));
    }

    public remove(saved: SavedQuery) {
        this.uiState.remove(`${this.prefix}.queries.${saved.name}`);
    }

    public getSaveKey(query$: Observable<string | undefined>): Observable<string | undefined> {
        return combineLatest(this.queries, query$).pipe(
            map(([queries, filter]) => {
                for (let query of queries) {
                    if (query.queryJson === filter) {
                        return query.name;
                    }
                }

                return undefined;
            }));
    }
}

export function parseStored(name: string, raw?: string) {
    if (Types.isString(raw)) {
        let query: Query;

        if (raw.indexOf('{') === 0) {
            query = JSON.parse(raw);
        } else {
            query = { fullText: raw };
        }

        return { name, query, queryJson: encodeQuery(query) };
    }

    return { name, query: undefined, queryJson: '' };
}