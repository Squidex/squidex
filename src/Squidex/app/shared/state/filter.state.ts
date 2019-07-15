/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/internal/operators';

import { State, Types } from '@app/framework';

import { LanguageDto } from './../services/languages.service';

import { RootFieldDto } from './../services/schemas.service';

interface Snapshot {
    // The current language.
    language?: LanguageDto;

    // The order statement.
    order?: string;

    // The order field.
    orderField?: string;

    // The order direction.
    orderDirection?: string;

    // The final query.
    query?: string;

    // The odata filter statement.
    filter?: string;

    // The odata full text statement.
    fullText?: string;
}

export type Sorting = 'Ascending' | 'Descending' | 'None';

type Field = string | RootFieldDto;

export class FilterState extends State<Snapshot> {
    private readonly sortModes: { [key: string]: Observable<Sorting> } = {};

    public query =
        this.project(x => x.query);

    public order =
        this.project(x => x.order);

    public filter =
        this.project(x => x.filter);

    public fullText =
        this.project(x => x.fullText);

    public get apiFilter() {
        return this.snapshot.query;
    }

    public constructor() {
        super({});
    }

    public sortMode(field: Field) {
        const key = Types.isString(field) ? field : field.fieldId.toString();

        let result = this.sortModes[key];

        if (!result) {
            result = this.project(x => sortMode(x, field)), distinctUntilChanged();

            this.sortModes[key] = result;
        }

        return result;
    }

    public setQuery(query?: string) {
        this.next(s => fromQuery(s, query));
    }

    public setOrder(order?: string) {
        this.next(s => fromProperty(s, { order }));
    }

    public setFilter(filter?: string) {
        this.next(s => fromProperty(s, { filter }));
    }

    public setFullText(fullText?: string) {
        this.next(s => fromProperty(s, { fullText }));
    }

    public setLanguage(language: LanguageDto) {
        this.next(s => ({ ...s, language }));
    }

    public setOrderField(field: Field, sorting: Sorting) {
        this.setOrder(getFieldSorting(this.snapshot, field, sorting));
    }
}

function sortMode(snapshot: Snapshot, field: Field): Sorting {
    let path = getFieldPath(snapshot, field);

    if (snapshot.orderField === path) {
        if (snapshot.orderDirection === 'asc') {
            return 'Ascending';
        } else if (snapshot.orderDirection === 'desc') {
            return 'Descending';
        }
    }

    return 'None';
}

function escapeField(value: string) {
    return value.replace('-', '_');
}

function getFieldSorting(snapshot: Snapshot, field: Field, sorting: Sorting) {
    if (sorting === 'Ascending') {
        return `${getFieldPath(snapshot, field)} asc`;
    } else {
        return `${getFieldPath(snapshot, field)} desc`;
    }
}

function getFieldPath(snapshot: Snapshot, field?: Field) {
    let path: string | undefined = undefined;

    if (field) {
        if (Types.isString(field)) {
            path = field;
        } else if (field.isLocalizable && snapshot.language) {
            path = `data/${escapeField(field.name)}/${snapshot.language.iso2Code}`;
        } else {
            path = `data/${escapeField(field.name)}/iv`;
        }
    }

    return path;
}

function fromQuery(previous: Snapshot, query?: string) {
    const snapshot: Snapshot = { language: previous.language, query };

    if (query) {
        const parts = query.split('&');

        if (parts.length === 1 && parts[0][0] !== '$') {
            snapshot.fullText = parts[0];
        } else {
            for (let part of parts) {
                const kvp = part.split('=');

                if (kvp.length === 2) {
                    const key = kvp[0].toLowerCase();

                    if (key === '$filter') {
                        snapshot.filter = kvp[1];
                    } else if (key === '$orderby') {
                        snapshot.order = kvp[1];
                    } else if (key === '$search') {
                        snapshot.fullText = kvp[1];
                    }
                }
            }
        }
    }

    return enrichOrderField(snapshot);
}

function fromProperty(previous: Snapshot, update: Partial<Snapshot>) {
    const snapshot = { ...previous, ...update };

    if (snapshot.fullText && !snapshot.order && !snapshot.filter) {
        snapshot.query = snapshot.fullText;
    } else {
        const parts: string[] = [];

        if (snapshot.fullText) {
            parts.push(`$search=${snapshot.fullText}`);
        }

        if (snapshot.filter) {
            parts.push(`$filter=${snapshot.filter}`);
        }

        if (snapshot.order) {
            parts.push(`$orderby=${snapshot.order}`);
        }

        snapshot.query = parts.join('&');
    }

    return enrichOrderField(snapshot);
}

function enrichOrderField(snapshot: Snapshot) {
    if (snapshot.order) {
        const orderParts = snapshot.order.split(' ');

        if (orderParts.length === 2) {
            snapshot.orderField = orderParts[0];
            snapshot.orderDirection = orderParts[1];
        }
    }

    return snapshot;
}