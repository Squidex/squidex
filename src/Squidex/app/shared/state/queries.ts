/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { UIState } from './ui.state';

export interface Query {
    name: string;
    nameSortable?: string;
    filter: string;
}

export class Queries {
    public queries: Observable<Query[]>;

    public defaultQueries: Query[] = [{
        name: 'All (newest first)', filter: ''
    }, {
        name: 'All (oldest first)', filter: '$orderby=lastModified asc'
    }];

    constructor(
        private readonly uiState: UIState,
        private readonly prefix: string
    ) {
        this.queries = this.uiState.get(`${this.prefix}.queries`, {}).pipe(
            map(x => {
                let queries: Query[] = Object.keys(x).map(y => ({ name: y, filter: x[y] }));

                for (let query of queries) {
                    query.nameSortable = query.name.toUpperCase();
                }

                queries = queries.sort((a, b) => {
                    if (a.nameSortable! < b.nameSortable!) {
                        return -1;
                    }
                    if (a.nameSortable! > b.nameSortable!) {
                        return 1;
                    }
                    return 0;
                });

                return queries;
            })
        );
    }

    public add(key: string, filter: string) {
        this.uiState.set(`${this.prefix}.queries.${key}`, filter);
    }

    public remove(key: string) {
        this.uiState.remove(`${this.prefix}.queries.${key}`);
    }

    public getSaveKey(filter$: Observable<string>): Observable<string | null> {
        return combineLatest(this.queries, filter$).pipe(
            map(project => {
                const filter = project[1];

                if (filter) {
                    for (let query of project[0]) {
                        if (query.filter === filter) {
                            return query.name;
                        }
                    }
                }
                return null;
            }));
    }
}