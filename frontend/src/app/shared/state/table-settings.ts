/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { map, take } from 'rxjs/operators';
import { State, Types } from '@app/framework';
import { META_FIELDS, SchemaDto, TableField } from '../services/schemas.service';
import { UIState } from './ui.state';

const META_FIELD_NAMES = Object.values(META_FIELDS).filter(x => x !== META_FIELDS.empty);

export type FieldSizes = { [name: string]: number };
export type FieldWrappings = { [name: string]: boolean };

interface Snapshot {
    // The table fields in the right order.
    fields: ReadonlyArray<TableField>;

    // The sizes of the columns if overriden.
    sizes: FieldSizes;

    // True to enable wrapping.
    wrappings: FieldWrappings;
}

export class TableSettings extends State<Snapshot> {
    private readonly settingsKey: string;

    public readonly schemaFields: ReadonlyArray<TableField>;
    public readonly schemaDefaults: ReadonlyArray<TableField>;

    public fieldSizes =
        this.project(x => x.sizes);

    public fieldWrappings =
        this.project(x => x.wrappings);

    public fields =
        this.project(x => x.fields);

    public listFields =
        this.projectFrom(this.fields, x => x.length > 0 ? x : this.schemaDefaults);

    constructor(
        public readonly uiState: UIState,
        public readonly schema: SchemaDto,
    ) {
        super({ fields: [], sizes: {}, wrappings: {} });

        this.schemaFields = [...schema.contentFields, ...META_FIELD_NAMES];
        this.schemaDefaults = schema.defaultListFields;

        this.settingsKey = `schemas.${this.schema.name}.config`;

        this.uiState.getAppUser<any>(this.settingsKey, {}).pipe(take(1))
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

    public updateFields(fields: ReadonlyArray<TableField>, save = true) {
        this.publishFields(fields.map(x => x.name));

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

    private publishFields(names: ReadonlyArray<string>) {
        const fields = names.map(n => this.schemaFields.find(f => f.name === n)).filter(x => !!x) as any;

        this.next({ fields });
    }

    private saveConfig() {
        const { sizes, fields, wrappings } = this.snapshot;

        if (Object.keys(sizes).length === 0 && Object.keys(wrappings).length === 0 && fields.length === 0) {
            this.uiState.removeAppUser(this.settingsKey);
        } else {
            const update = { sizes, wrappings, fields: fields.map(x => x.name) };

            this.uiState.setAppUser(this.settingsKey, update);
        }
    }
}

export function getTableConfig(source: TableSettings) {
    function sortedTableFields(fields: ReadonlyArray<TableField>) {
        const result: string[] = [];

        for (const field of fields) {
            if (field.name && field.name.indexOf('meta') < 0) {
                result.push(field.name);
            }
        }

        result.sort();
        return result;
    }

    return source.listFields.pipe(
        map(fields => sortedTableFields(fields)),
        map(fields => ({ fieldNames: fields, schema: source.schema })));
}