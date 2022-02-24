/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { take } from 'rxjs/operators';
import { State, Types } from '@app/framework';
import { MetaFields, SchemaDto } from './../services/schemas.service';
import { UIState } from './ui.state';

const META_FIELD_NAMES = Object.values(MetaFields);

export type FieldSizes = { [name: string]: number };
export type FieldWrappings = { [name: string]: boolean };

interface Snapshot {
    // The table fields in the right order.
    fields: ReadonlyArray<string>;

    // The sizes of the columns if overriden.
    sizes: FieldSizes;

    // True to enable wrapping.
    wrappings: FieldWrappings;
}

export class TableSettings extends State<Snapshot> {
    private readonly settingsKey: string;

    public readonly schemaFields: ReadonlyArray<string>;
    public readonly schemaDefaults: ReadonlyArray<string>;

    public fieldSizes =
        this.project(x => x.sizes);

    public fieldWrappings =
        this.project(x => x.wrappings);
        
    public configuredFields =
        this.project(x => x.fields);

    public listFieldNames =
        this.projectFrom(this.configuredFields, x => x.length === 0 ? this.schemaDefaults : x);

    public listFields =
        this.projectFrom(this.listFieldNames, x => x.map(n => this.schema.fields.find(f => f.name === n) || n));

    constructor(
        private readonly uiState: UIState,
        private readonly schema: SchemaDto,
    ) {
        super({ fields: [], sizes: {}, wrappings: {} });

        this.schemaFields = [...schema.contentFields.map(x => x.name), ...META_FIELD_NAMES].sort();
        this.schemaDefaults = schema.defaultListFields.map(x => x['name'] || x);

        this.settingsKey = `schemas.${this.schema.name}.config`;

        this.uiState.getUser<any>(this.settingsKey, {}).pipe(take(1))
            .subscribe(settings => {
                if (!Types.isArrayOfString(settings.fields)) {
                    settings.fields = [];
                }

                if (!Types.isObject(settings.sizes)) {
                    settings.sizes = {};
                }

                if (!Types.isObject(settings.wrappings)) {
                    settings.wrappings = {};
                }

                this.publishSizes(settings.sizes);
                this.publishFields(settings.fields);
                this.publishWrappings(settings.wrappings);
            });
    }

    public reset() {
        super.resetState();

        this.saveConfig();
    }

    public updateSize(field: string, size: number, save = true) {
        this.next(s => ({ 
            ...s,
            sizes: { 
                ...s.sizes, 
                [field]: size,
            },
        }));

        if (save) {
            this.saveConfig();
        }
    }

    public toggleWrapping(field: string, save = true) {
        this.next(s => ({ 
            ...s,
            wrappings: { 
                ...s.wrappings, 
                [field]: !s.wrappings[field],
            },
        }));

        if (save) {
            this.saveConfig();
        }
    }

    public updateFields(fields: ReadonlyArray<string>, save = true) {
        this.publishFields(fields);

        if (save) {
            this.saveConfig();
        }
    }

    private publishSizes(sizes: FieldSizes) {
        this.next({ sizes });    
    }

    private publishWrappings(wrappings: FieldWrappings) {
        this.next({ wrappings });    
    }

    private publishFields(fields: ReadonlyArray<string>) {
        this.next({ fields: fields.filter(x => this.schemaFields.includes(x)) });
    }

    private saveConfig() {
        const { sizes, fields, wrappings: wraps } = this.snapshot;

        if (Object.keys(sizes).length === 0 && Object.keys(wraps).length === 0 && fields.length === 0) {
            this.uiState.removeUser(this.settingsKey);                
        } else {
            this.uiState.set(this.settingsKey, this.snapshot, true);
        }
    }
}
