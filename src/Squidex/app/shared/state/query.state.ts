/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';

import { State, Types } from '@app/framework';

import { LanguageDto } from './../services/languages.service';
import { RootFieldDto } from './../services/schemas.service';
import { encodeQuery, Query, SortMode } from './query';

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
    query?: Query;
}

type Field = string | RootFieldDto;

export class QueryState extends State<Snapshot> {
    private readonly sortModes: { [key: string]: Observable<SortMode | null> } = {};

    public query =
        this.project(x => x.query);

    public fullText =
        this.project(x => x.query ? x.query.fullText : '');

    public queryJson =
        this.project(x => encodeQuery(x.query));

    public constructor() {
        super({});
    }

    public sortMode(field: Field) {
        const key = Types.isString(field) ? field : field.fieldId.toString();

        let result = this.sortModes[key];

        if (!result) {
            result = this.project(x => getSortMode(x.query, x.language, field));

            this.sortModes[key] = result;
        }

        return result;
    }

    public setFullText(fullText?: string) {
        this.next(s => ({ ...s, query: { ...s.query, fullText }}));
    }

    public setQuery(query?: Query) {
        this.next(s => ({ ...s, query }));
    }

    public setLanguage(language: LanguageDto) {
        this.next(s => ({ ...s, language }));
    }

    public setOrderField(field: Field, order: SortMode) {
        this.next(s => {
            let query: Query = { ...s.query };

            const path = getFieldPath(s.language, field);

            if (path) {
                query.sort = [
                    { path, order }
                ];
            }

            return { ...s, query };
        });
    }
}

function getSortMode(query: Query | undefined, language: LanguageDto | undefined, field: Field) {
    if (query && query.sort) {
        const path = getFieldPath(language, field);

        if (path) {
            if (query.sort.length === 1 && query.sort[0].path === path) {
                return query.sort[0].order;
            }
        }
    }

    return null;
}

function getFieldPath(language: LanguageDto | undefined, field: Field) {
    let path: string | undefined = undefined;

    if (field) {
        if (Types.isString(field)) {
            path = field;
        } else if (field.isLocalizable && language) {
            path = `data.${field.name}.${language.iso2Code}`;
        } else {
            path = `data.${field.name}.iv`;
        }
    }

    return path;
}