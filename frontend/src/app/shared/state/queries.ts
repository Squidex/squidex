/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Signal } from '@angular/core';
import { compareStrings, Types } from '@app/framework';
import { deserializeQuery, equalsQuery, Query } from '../services/query';
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
        { path: 'lastModified', order: 'ascending' },
    ],
};

export class Queries {
    public readonly queries: Signal<ReadonlyArray<SavedQuery>>;
    public readonly queriesShared: Signal<ReadonlyArray<SavedQuery>>;
    public readonly queriesUser: Signal<ReadonlyArray<SavedQuery>>;

    public readonly defaultQueries: ReadonlyArray<SavedQuery> = [
        { name: 'i18n:search.queryAllNewestFirst' },
        { name: 'i18n:search.queryAllOldestFirst', query: OLDEST_FIRST },
    ];

    constructor(
        private readonly uiState: UIState,
        private readonly prefix: string,
    ) {
        const path = `${prefix}.queries`;

        this.queries = parseQueriesFrom(this.uiState.get(path, {}));
        this.queriesShared = parseQueriesFrom(this.uiState.getAppShared(path, {}));
        this.queriesUser = parseQueriesFrom(this.uiState.getAppUser(path, {}));
    }

    public addUser(key: string, query: Query) {
        this.uiState.setAppUser(this.getPath(key), JSON.stringify(query));
    }

    public addShared(key: string, query: Query) {
        this.uiState.setAppShared(this.getPath(key), JSON.stringify(query));
    }

    public removeShared(saved: SavedQuery | string) {
        this.uiState.removeAppShared(this.getPath(saved));
    }

    public removeUser(saved: SavedQuery | string) {
        this.uiState.removeAppUser(this.getPath(saved));
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

    public getSaveKey(query: Query): string | undefined {
        for (const saved of this.queries()) {
            if (equalsQuery(saved.query, query)) {
                return saved.name;
            }
        }

        return undefined;
    }
}

function parseQueriesFrom(settings: Signal<{}>) {
    return computed(() => {
        return parseQueries(settings());
    });
}

function parseQueries(settings: {}) {
    const queries = Object.entries(settings).map(([name, value]) => parseStored(name, value as any));

    return queries.sort((a, b) => compareStrings(a.name, b.name));
}

export function parseStored(name: string, raw?: string) {
    const query = deserializeQuery(raw);

    return { name, query };
}
