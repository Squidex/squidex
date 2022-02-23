/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject, Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { MetaFields, SchemaDto, TableField } from './../services/schemas.service';
import { UIState } from './ui.state';

const META_FIELD_NAMES = Object.values(MetaFields);

export type TableSizes = { [name: string]: number };

export class TableFields {
    private readonly listSizes$ = new BehaviorSubject<TableSizes>({});
    private readonly listField$ = new BehaviorSubject<ReadonlyArray<TableField>>([]);
    private readonly listFieldName$ = new BehaviorSubject<ReadonlyArray<string>>([]);
    private readonly settingsKeyNames: string;
    private readonly settingsKeySizes: string;

    public readonly allFields: ReadonlyArray<string>;

    public get listFields(): Observable<ReadonlyArray<TableField>> {
        return this.listField$;
    }

    public get listFieldNames(): Observable<ReadonlyArray<string>> {
        return this.listFieldName$;
    }

    public get listSizes(): Observable<TableSizes> {
        return this.listSizes$;
    }

    constructor(
        private readonly uiState: UIState,
        private readonly schema: SchemaDto,
    ) {
        this.allFields = [...this.schema.contentFields.map(x => x.name), ...META_FIELD_NAMES].sort();

        this.settingsKeyNames = `schemas.${this.schema.name}.view`;
        this.settingsKeySizes = `schemas.${this.schema.name}.sizes`;

        this.uiState.getUser<string[]>(this.settingsKeyNames, []).pipe(take(1))
            .subscribe(fieldNames => {
                this.updateFields(fieldNames, false);
            });

        this.uiState.getUser<TableSizes>(this.settingsKeySizes, {}).pipe(take(1))
            .subscribe(fieldSizes => {
                this.listSizes$.next(fieldSizes);
            });
    }

    public updateSize(fieldName: string, size: number, save = true) {
        let sizes = { ...this.listSizes$.value };

        sizes[fieldName] = size;

        if (save) {
            this.uiState.set(this.settingsKeySizes, sizes, true);
        }

        this.listSizes$.next(sizes);
    }

    public updateFields(fieldNames: ReadonlyArray<string>, save = true) {
        fieldNames = fieldNames.filter(x => this.allFields.indexOf(x) >= 0);

        if (fieldNames.length === 0) {
            fieldNames = this.schema.defaultListFields.map(x => x['name'] || x);

            if (save) {
                this.uiState.removeUser(this.settingsKeyNames);
            }
        } else if (save) {
            this.uiState.set(this.settingsKeyNames, fieldNames, true);
        }

        const fields: ReadonlyArray<TableField> = fieldNames.map(n => this.schema.fields.find(f => f.name === n) || n);

        this.listField$.next(fields);
        this.listFieldName$.next(fieldNames);
    }
}
