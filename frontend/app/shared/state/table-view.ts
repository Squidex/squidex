/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { BehaviorSubject, Observable } from 'rxjs';
import { take } from 'rxjs/operators';

import {
    MetaFields,
    SchemaDetailsDto,
    TableField
} from '../services/schemas.service';

import { UIState } from './ui.state';

const META_FIELD_NAMES = Object.values(MetaFields);

export class TableView {
    private readonly listField$ = new BehaviorSubject<ReadonlyArray<TableField>>([]);
    private readonly listFieldName$ = new BehaviorSubject<ReadonlyArray<string>>([]);

    public readonly allFields: ReadonlyArray<string>;

    public get listFields(): Observable<ReadonlyArray<TableField>> {
        return this.listField$;
    }

    public get listFieldNames(): Observable<ReadonlyArray<string>> {
        return this.listFieldName$;
    }

    constructor(
        private readonly uiState: UIState,
        private readonly schema: SchemaDetailsDto
    ) {
        this.allFields = [...this.schema.contentFields.map(x => x.name), ...META_FIELD_NAMES].sorted();

        this.uiState.getUser<string[]>(`${this.schema.id}.view`, []).pipe(take(1))
            .subscribe(fieldNames => {
                this.updateFields(fieldNames, false);
            });
    }

    public updateFields(fieldNames: string[], save = true) {
        fieldNames = fieldNames.filter(x => this.allFields.indexOf(x) >= 0);

        if (fieldNames.length === 0) {
            fieldNames = this.schema.defaultListFields.map(x => x['name'] || x);
        }

        if (save) {
            this.uiState.set(`${this.schema.id}.view`, fieldNames, true);
        }

        const fields: ReadonlyArray<TableField> = fieldNames.map(n => this.schema.fields.find(f => f.name === n) || n);

        this.listField$.next(fields);
        this.listFieldName$.next(fieldNames);
    }

    public resetDefault() {
        this.updateFields([]);
    }

    public addField(field: string) {
        this.updateFields([...this.listFieldName$.value, field]);
    }

    public removeField(field: string) {
        this.updateFields(this.listFieldName$.value.filter(x => x !== field));
    }
}