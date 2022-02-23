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
export type TableSettings = { fields?: ReadonlyArray<string>; sizes?: TableSizes };

export class TableFields {
    private readonly listSizes$ = new BehaviorSubject<TableSizes>({});
    private readonly listFields$ = new BehaviorSubject<ReadonlyArray<TableField>>([]);
    private readonly listFieldName$ = new BehaviorSubject<ReadonlyArray<string>>([]);
    private readonly settingsKey: string;
    private settings: TableSettings = {};

    public readonly allFields: ReadonlyArray<string>;

    public get listFields(): Observable<ReadonlyArray<TableField>> {
        return this.listFields$;
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

        this.settingsKey = `schemas.${this.schema.name}.config`;

        this.uiState.getUser<TableSettings>(this.settingsKey, {}).pipe(take(1))
            .subscribe(settings => {
                this.settings = settings;
                
                this.updateByConfig(false);
            });
    }

    public reset() {
        this.settings = {};

        this.updateByConfig(true);
    }

    public updateSize(fieldName: string, size: number, save = true) {
        this.settings.sizes = { ...this.listSizes$.value, [fieldName]: size };

        this.updateByConfig(save);
    }

    public updateFields(fieldNames: ReadonlyArray<string>, save = true) {
        this.settings.fields = fieldNames.filter(x => this.allFields.indexOf(x) >= 0);

        this.updateByConfig(save);
    }

    private updateByConfig(save = true) {
        let { fields, sizes } = this.settings;

        if (!fields) {
            fields = [];
        } else {
            fields = fields.filter(x => this.allFields.indexOf(x) >= 0);

            this.settings.fields = [...fields];
        }

        if (!sizes) {
            sizes = {};
        }

        if (save) {
            if (Object.keys(sizes).length === 0 && fields.length === 0) {
                this.uiState.removeUser(this.settingsKey);                
            } else {
                this.uiState.set(this.settingsKey, { sizes, fields }, true);
            }
        }

        if (fields.length === 0) {
            fields = this.schema.defaultListFields.map(x => x['name'] || x);
        }

        const tableFields = fields.map(n => this.schema.fields.find(f => f.name === n) || n);

        this.listSizes$.next(sizes);
        this.listFields$.next(tableFields);
        this.listFieldName$.next(fields);
    }
}
