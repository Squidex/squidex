/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    mapVersioned,
    shareSubscribed,
    State,
    Types,
    Version
} from '@app/framework';

import { AuthService } from './../services/auth.service';
import { AppsState } from './apps.state';

import {
    AddFieldDto,
    CreateSchemaDto,
    FieldDto,
    NestedFieldDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaDto
} from './../services/schemas.service';

import { FieldPropertiesDto } from './../services/schemas.types';

type AnyFieldDto = NestedFieldDto | RootFieldDto;

interface Snapshot {
    // The schema categories.
    categories: { [name: string]: boolean };

    // The current schemas.
    schemas: SchemasList;

    // Indicates if the schemas are loaded.
    isLoaded?: boolean;

    // The selected schema.
    selectedSchema?: SchemaDetailsDto | null;
}

export type SchemasList = ImmutableArray<SchemaDto>;

function sameSchema(lhs: SchemaDetailsDto | null, rhs?: SchemaDetailsDto | null): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id && lhs.version === rhs.version);
}

@Injectable()
export class SchemasState extends State<Snapshot> {
    public get schemaName() {
        return this.snapshot.selectedSchema ? this.snapshot.selectedSchema.name : '';
    }

    public selectedSchema =
        this.changes.pipe(map(x => x.selectedSchema),
            distinctUntilChanged(sameSchema));

    public categories =
        this.changes.pipe(map(x => ImmutableArray.of(Object.keys(x.categories)).sortByStringAsc(s => s)),
            distinctUntilChanged());

    public schemas =
        this.changes.pipe(map(x => x.schemas),
            distinctUntilChanged());

    public publishedSchemas =
        this.changes.pipe(map(x => x.schemas.filter(s => s.isPublished)),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly schemasService: SchemasService
    ) {
        super({ schemas: ImmutableArray.empty(), categories: {} });
    }

    public select(idOrName: string | null): Observable<SchemaDetailsDto | null> {
        return this.loadSchema(idOrName).pipe(
            tap(selectedSchema => {
                this.next(s => {
                    const schemas = selectedSchema ? s.schemas.replaceBy('id', selectedSchema) : s.schemas;

                    return { ...s, selectedSchema, schemas };
                });
            }));
    }

    private loadSchema(idOrName: string | null) {
        return !idOrName ? of(null) :
            this.schemasService.getSchema(this.appName, idOrName).pipe(
                catchError(() => of(null)));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.schemasService.getSchemas(this.appName).pipe(
            tap(payload => {
                if (isReload) {
                    this.dialogs.notifyInfo('Schemas reloaded.');
                }

                return this.next(s => {
                    const schemas = ImmutableArray.of(payload).sortByStringAsc(x => x.displayName);

                    const categories = buildCategories(s.categories, schemas);

                    return { ...s, schemas, isLoaded: true, categories };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateSchemaDto, now?: DateTime): Observable<SchemaDetailsDto> {
        return this.schemasService.postSchema(this.appName, request, this.user, now || DateTime.now()).pipe(
            tap(dto => {
                this.next(s => {
                    const schemas = s.schemas.push(dto).sortByStringAsc(x => x.displayName);

                    return { ...s, schemas };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public delete(schema: SchemaDto): Observable<any> {
        return this.schemasService.deleteSchema(this.appName, schema.name, schema.version).pipe(
            tap(() => {
                this.next(s => {
                    const schemas = s.schemas.filter(x => x.id !== schema.id);
                    const selectedSchema = s.selectedSchema && s.selectedSchema.id === schema.id ? null : s.selectedSchema;

                    return { ...s, schemas, selectedSchema };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public addCategory(name: string) {
        this.next(s => {
            const categories = addCategory(s.categories, name);

            return { ...s, categories: categories };
        });
    }

    public removeCategory(name: string) {
        this.next(s => {
            const categories = removeCategory(s.categories, name);

            return { ...s, categories: categories };
        });
    }

    public publish(schema: SchemaDto, now?: DateTime): Observable<SchemaDto> {
        return this.schemasService.publishSchema(this.appName, schema.name, schema.version).pipe(
            map(({ version }) => setPublished(schema, true, this.user, version, now)),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public unpublish(schema: SchemaDto, now?: DateTime): Observable<SchemaDto> {
        return this.schemasService.unpublishSchema(this.appName, schema.name, schema.version).pipe(
            map(({ version }) => setPublished(schema, false, this.user, version, now)),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public changeCategory(schema: SchemaDto, name: string, now?: DateTime): Observable<SchemaDto> {
        return this.schemasService.putCategory(this.appName, schema.name, { name }, schema.version).pipe(
            map(({ version }) => changeCategory(schema, name, this.user, version, now)),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public configurePreviewUrls(schema: SchemaDetailsDto, request: {}, now?: DateTime): Observable<SchemaDetailsDto> {
        return this.schemasService.putPreviewUrls(this.appName, schema.name, request, schema.version).pipe(
            map(({ version }) => configurePreviewUrls(schema, request, this.user, version, now)),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public configureScripts(schema: SchemaDetailsDto, request: {}, now?: DateTime): Observable<SchemaDetailsDto> {
        return this.schemasService.putScripts(this.appName, schema.name, request, schema.version).pipe(
            map(({ version }) => configureScripts(schema, request, this.user, version, now)),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(schema: SchemaDetailsDto, request: UpdateSchemaDto, now?: DateTime): Observable<SchemaDetailsDto> {
        return this.schemasService.putSchema(this.appName, schema.name, request, schema.version).pipe(
            map(({ version }) => updateProperties(schema, request, this.user, version, now)),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public addField(schema: SchemaDetailsDto, request: AddFieldDto, parent?: RootFieldDto, now?: DateTime): Observable<FieldDto> {
        return this.schemasService.postField(this.appName, schema.name, request, pid(parent), schema.version).pipe(
            map(({ version, payload }) => {
                let newSchema: SchemaDto;

                if (Types.is(payload, NestedFieldDto)) {
                    newSchema = updateField(schema, addNested(parent!, payload), this.user, version, now);
                } else {
                    newSchema = addField(schema, payload, this.user, version, now);
                }

                return {  newSchema, field: payload };
            }),
            tap(({ newSchema }) => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs, { silent: true, project: x => x.field }));
    }

    public sortFields(schema: SchemaDetailsDto, fields: any[], parent?: RootFieldDto, now?: DateTime): Observable<SchemaDetailsDto> {
        return this.schemasService.putFieldOrdering(this.appName, schema.name, fields.map(t => t.fieldId), pid(parent), schema.version).pipe(
            map(({ version }) => {
                let newSchema: SchemaDto;

                if (!parent) {
                    newSchema = replaceFields(schema, fields, this.user, version, now);
                } else {
                    newSchema = updateField(schema, replaceNested(parent, fields), this.user, version, now);
                }

                return newSchema;
            }),
            tap(newSchema => {
                this.replaceSchema(newSchema);
            }),
            shareSubscribed(this.dialogs));
    }

    public lockField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, now?: DateTime): Observable<T> {
        return this.schemasService.lockField(this.appName, schema.name, field.fieldId, pidof(field), schema.version).pipe(
            mapVersioned(() => setLocked(field, true)),
            tap(({ payload, version }) => {
                this.replaceField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public enableField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, now?: DateTime): Observable<T> {
        return this.schemasService.enableField(this.appName, schema.name, field.fieldId, pidof(field), schema.version).pipe(
            mapVersioned(() => setDisabled(field, false)),
            tap(({ payload, version }) => {
                this.replaceField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public disableField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, now?: DateTime): Observable<T> {
        return this.schemasService.disableField(this.appName, schema.name, field.fieldId, pidof(field), schema.version).pipe(
            mapVersioned(() => setDisabled(field, true)),
            tap(({ payload, version }) => {
                this.replaceField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public showField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, now?: DateTime): Observable<T> {
        return this.schemasService.showField(this.appName, schema.name, field.fieldId, pidof(field), schema.version).pipe(
            mapVersioned(() => setHidden(field, false)),
            tap(({ payload, version }) => {
                this.replaceField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public hideField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, now?: DateTime): Observable<T> {
        return this.schemasService.hideField(this.appName, schema.name, field.fieldId, pidof(field), schema.version).pipe(
            mapVersioned(() => setHidden(field, true)),
            tap(({ payload, version }) => {
                this.replaceField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public updateField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, request: UpdateFieldDto, now?: DateTime): Observable<T> {
        return this.schemasService.putField(this.appName, schema.name, field.fieldId, request, pidof(field), schema.version).pipe(
            mapVersioned(() => update(field, request.properties)),
            tap(({ payload, version }) => {
                this.replaceField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload }));
    }

    public deleteField(schema: SchemaDetailsDto, field: AnyFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.deleteField(this.appName, schema.name, field.fieldId, pidof(field), schema.version).pipe(
            mapVersioned(() => field),
            tap(({ payload, version }) => {
                this.removeField(schema, payload, version, now);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceField<T extends FieldDto>(schema: SchemaDetailsDto, field: T, version: Version, now?: DateTime) {
        if (Types.is(field, RootFieldDto)) {
            this.replaceSchema(updateField(schema, field, this.user, version, now));
        } else if (Types.is(field, NestedFieldDto)) {
            const parent = schema.fields.find(x => x.fieldId === field.parentId);

            if (parent) {
                this.replaceSchema(updateField(schema, updatedNested(parent, field), this.user, version, now));
            }
        }
    }

    private removeField(schema: SchemaDetailsDto, field: AnyFieldDto, version: Version, now?: DateTime) {
        if (Types.is(field, RootFieldDto)) {
            this.replaceSchema(removeField(schema, field, this.user, version, now));
        } else {
            const parent = schema.fields.find(x => x.fieldId === field.parentId);

            if (parent) {
                this.replaceSchema(updateField(schema, removeNested(parent, field), this.user, version, now));
            }
        }
    }

    private replaceSchema(schema: SchemaDto) {
        return this.next(s => {
            const schemas = s.schemas.replaceBy('id', schema).sortByStringAsc(x => x.displayName);
            const selectedSchema = Types.is(schema, SchemaDetailsDto) && s.selectedSchema && s.selectedSchema.id === schema.id ? schema : s.selectedSchema;

            const categories = buildCategories(s.categories, schemas);

            return { ...s, schemas, selectedSchema, categories };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get user() {
        return this.authState.user!.token;
    }
}

function buildCategories(categories: { [name: string]: boolean }, schemas: SchemasList) {
    categories = { ...categories };

    for (let category in categories) {
        if (categories.hasOwnProperty(category)) {
            if (!categories[category]) {
                delete categories[category];
            }
        }
    }
    for (let schema of schemas.values) {
        categories[schema.category || ''] = false;
    }

    return categories;
}

function addCategory(categories: { [name: string]: boolean }, category: string) {
    categories = { ...categories };

    categories[category] = true;

    return categories;
}

function removeCategory(categories: { [name: string]: boolean }, category: string) {
    categories = { ...categories };

    delete categories[category];

    return categories;
}

const setPublished = <T extends SchemaDto>(schema: T, isPublished: boolean, user: string, version: Version, now?: DateTime) =>
    <T>schema.with({
        isPublished,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const changeCategory = <T extends SchemaDto>(schema: T, category: string, user: string, version: Version, now?: DateTime) =>
    <T>schema.with({
        category,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const configurePreviewUrls = (schema: SchemaDetailsDto, previewUrls: {}, user: string, version: Version, now?: DateTime) =>
    schema.with({
        previewUrls,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const configureScripts = (schema: SchemaDetailsDto, scripts: {}, user: string, version: Version, now?: DateTime) =>
    schema.with({
        scripts,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const updateProperties = (schema: SchemaDetailsDto, properties: SchemaPropertiesDto, user: string, version: Version, now?: DateTime) =>
    schema.with({
        properties,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const addField = (schema: SchemaDetailsDto, field: RootFieldDto, user: string, version: Version, now?: DateTime) =>
    schema.with({
        fields: [...schema.fields, field],
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const updateField = (schema: SchemaDetailsDto, field: RootFieldDto, user: string, version: Version, now?: DateTime) =>
    schema.with({
        fields: schema.fields.map(f => f.fieldId === field.fieldId ? field : f),
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const replaceFields = (schema: SchemaDetailsDto, fields: RootFieldDto[], user: string, version: Version, now?: DateTime) =>
    schema.with({
        fields,
        version,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user
    });

const removeField = (schema: SchemaDetailsDto, field: FieldDto, user: string, version: Version, now?: DateTime) =>
    schema.with({
        fields: schema.fields.filter(f => f.fieldId !== field.fieldId),
        version,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user
    });

const addNested = (parent: RootFieldDto, nested: NestedFieldDto) =>
    parent.with({ nested: [...parent.nested, nested] });

const updatedNested = (parent: RootFieldDto, nested: NestedFieldDto) =>
    parent.with({ nested: parent.nested.map(f => f.fieldId === nested.fieldId ? nested : f) });

const replaceNested = (parent: RootFieldDto, nested: NestedFieldDto[]) =>
    parent.with({ nested });

const removeNested = (parent: RootFieldDto, nested: NestedFieldDto) =>
    parent.with({ nested: parent.nested.filter(f => f.fieldId !== nested.fieldId) });

const setLocked = <T extends FieldDto>(field: T, isLocked: boolean) =>
    <T>field.with({ isLocked });

const setHidden = <T extends FieldDto>(field: T, isHidden: boolean) =>
    <T>field.with({ isHidden });

const setDisabled = <T extends FieldDto>(field: T, isDisabled: boolean) =>
    <T>field.with({ isDisabled });

const update = <T extends FieldDto>(field: T, properties: FieldPropertiesDto) =>
    <T>field.with({ properties });

const pidof = <T extends FieldDto>(field: T) =>
    Types.is(field, NestedFieldDto) ? field.parentId : undefined;

const pid = (field?: RootFieldDto) =>
    field ? field.fieldId : undefined;