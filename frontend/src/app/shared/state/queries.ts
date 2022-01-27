/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { compareStrings, Types } from '@app/framework';
import { deserializeQuery, equalsQuery, Query } from './../services/query';
import { UIState } from './ui.state';

export interface SavedQuery {
    // The optional color.
    color?: string;

    // The name of the query.
    name: string;

    // The deserialized value.
    query?: Query;
}

const OLDEST_FIRST: Query = {
    sort: [
        { path: 'lastModified', order: 'descending' },
    ],
};

export class Queries {
    public queries: Observable<ReadonlyArray<SavedQuery>>;
    public queriesShared: Observable<ReadonlyArray<SavedQuery>>;
    public queriesUser: Observable<ReadonlyArray<SavedQuery>>;

    public defaultQueries: ReadonlyArray<SavedQuery> = [
        { name: 'i18n:search.queryAllNewestFirst' },
        { name: 'i18n:search.queryAllOldestFirst', query: OLDEST_FIRST },
    ];

    constructor(
        private readonly uiState: UIState,
        private readonly prefix: string,
    ) {
        const path = `${prefix}.queries`;

        this.queries = this.uiState.get(path, {}).pipe(
            map(parseQueries), shareReplay(1));

        this.queriesShared = this.uiState.getShared(path, {}).pipe(
            map(parseQueries), shareReplay(1));

        this.queriesUser = this.uiState.getUser(path, {}).pipe(
            map(parseQueries), shareReplay(1));
    }

    public add(key: string, query: Query, user = false) {
        this.uiState.set(this.getPath(key), JSON.stringify(query), user);
    }

    public removeShared(saved: SavedQuery | string) {
        this.uiState.removeShared(this.getPath(saved));
    }

    public removeUser(saved: SavedQuery | string) {
        this.uiState.removeUser(this.getPath(saved));
    }

    public remove(saved: SavedQuery | string) {
        this.uiState.remove(this.getPath(saved));
    }

    private getPath(saved: SavedQuery | string): string {
        let key: string;

        if (Types.isString(saved)) {
            key = saved;
        } else {
            key = saved.name;
        }

        return `${this.prefix}.queries.${key}`;
    }

    public getSaveKey(query: Query): Observable<string | undefined> {
        return this.queries.pipe(
            map(queries => {
                for (const saved of queries) {
                    if (equalsQuery(saved.query, query)) {
                        return saved.name;
                    }
                }

                return undefined;
            }));
    }
}

function parseQueries(settings: {}) {
    const queries = Object.entries(settings).map(([name, value]) => parseStored(name, value as any));

    return queries.sort((a, b) => compareStrings(a.name, b.name));
}

export function parseStored(name: string, raw?: string) {
    const query = deserializeQuery(raw);

    return { name, query };
}
