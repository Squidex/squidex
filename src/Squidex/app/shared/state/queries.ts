/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

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
    public queries: Observable<ReadonlyArray<SavedQuery>>;
    public queriesShared: Observable<ReadonlyArray<SavedQuery>>;
    public queriesUser: Observable<ReadonlyArray<SavedQuery>>;

    public defaultQueries: ReadonlyArray<SavedQuery> = [
        { name: 'All (newest first)', queryJson: '' },
        { name: 'All (oldest first)', queryJson: encodeQuery(OLDEST_FIRST), query: OLDEST_FIRST }
    ];

    constructor(
        private readonly uiState: UIState,
        private readonly prefix: string
    ) {
        const path = `${prefix}.queries`;

        this.queries = this.uiState.get(path, {}).pipe(
            map(settings => parseQueries(settings)), shareReplay(1));

        this.queriesShared = this.uiState.getShared(path, {}).pipe(
            map(settings => parseQueries(settings)), shareReplay(1));

        this.queriesUser = this.uiState.getUser(path, {}).pipe(
            map(settings => parseQueries(settings)), shareReplay(1));
    }

    public add(key: string, query: Query, user = false) {
        this.uiState.set(this.getPath(key), JSON.stringify(query), user);
    }

    public removeShared(saved: SavedQuery) {
        this.uiState.removeShared(this.getPath(saved.name));
    }

    public removeUser(saved: SavedQuery) {
        this.uiState.removeUser(this.getPath(saved.name));
    }

    public remove(saved: SavedQuery) {
        this.uiState.remove(this.getPath(saved.name));
    }

    private getPath(key: string): string {
        return `${this.prefix}.queries.${key}`;
    }

    public getSaveKey(query: Query): Observable<string | undefined> {
        const json = encodeQuery(query);

        return this.queries.pipe(
            map(queries => {
                for (const saved of queries) {
                    if (saved.queryJson === json) {
                        return saved.name;
                    }
                }

                return undefined;
            }));
    }
}

function parseQueries(settings: {}) {
    let queries = Object.keys(settings).map(name => parseStored(name, settings[name]));

    return queries.sort((a, b) => compareStringsAsc(a.name, b.name));
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