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

export class TableFields {
    private readonly listField$ = new BehaviorSubject<ReadonlyArray<TableField>>([]);
    private readonly listFieldName$ = new BehaviorSubject<ReadonlyArray<string>>([]);
    private readonly settingsKey: string;

    public readonly allFields: ReadonlyArray<string>;

    public get listFields(): Observable<ReadonlyArray<TableField>> {
        return this.listField$;
    }

    public get listFieldNames(): Observable<ReadonlyArray<string>> {
        return this.listFieldName$;
    }

    constructor(
        private readonly uiState: UIState,
        private readonly schema: SchemaDto,
    ) {
        this.allFields = [...this.schema.contentFields.map(x => x.name), ...META_FIELD_NAMES].sort();

        this.settingsKey = `schemas.${this.schema.name}.view`;

        this.uiState.getUser<string[]>(this.settingsKey, []).pipe(take(1))
            .subscribe(fieldNames => {
                this.updateFields(fieldNames, false);
            });
    }

    public updateFields(fieldNames: ReadonlyArray<string>, save = true) {
        fieldNames = fieldNames.filter(x => this.allFields.indexOf(x) >= 0);

        if (fieldNames.length === 0) {
            fieldNames = this.schema.defaultListFields.map(x => x['name'] || x);

            if (save) {
                this.uiState.removeUser(this.settingsKey);
            }
        } else if (save) {
            this.uiState.set(this.settingsKey, fieldNames, true);
        }

        const fields: ReadonlyArray<TableField> = fieldNames.map(n => this.schema.fields.find(f => f.name === n) || n);

        this.listField$.next(fields);
        this.listFieldName$.next(fieldNames);
    }
}
