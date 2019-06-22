/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import {
    compareStringsAsc,
    DialogService,
    ImmutableArray,
    shareMapSubscribed,
    shareSubscribed,
    State,
    Types
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    AddFieldDto,
    CreateSchemaDto,
    FieldDto,
    NestedFieldDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaDto
} from './../services/schemas.service';

type AnyFieldDto = NestedFieldDto | RootFieldDto;

interface Snapshot {
    // The schema categories.
    categories: string[];

    // The current schemas.
    schemas: SchemasList;

    // Indicates if the schemas are loaded.
    isLoaded?: boolean;

    // The selected schema.
    selectedSchema?: SchemaDetailsDto | null;

    // Indicates if the user can create a schema.
    canCreate?: boolean;
}

export type SchemasList = ImmutableArray<SchemaDto>;
export type SchemaCategory = { name: string; schemas: SchemasList; upper: string; };

function sameSchema(lhs: SchemaDetailsDto | null, rhs?: SchemaDetailsDto | null): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id && lhs.version === rhs.version);
}

@Injectable()
export class SchemasState extends State<Snapshot> {
    public get schemaName() {
        return this.snapshot.selectedSchema ? this.snapshot.selectedSchema.name : '';
    }

    public categories =
        this.project2(x => x, x => buildCategories(x.categories, x.schemas));

    public selectedSchema =
        this.project(x => x.selectedSchema, sameSchema);

    public schemas =
        this.project(x => x.schemas);

    public publishedSchemas =
        this.project2(x => x.schemas, x => x.filter(s => s.isPublished));

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly schemasService: SchemasService
    ) {
        super({ schemas: ImmutableArray.empty(), categories: [] });
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
            const selectedSchema = this.snapshot.selectedSchema;

            this.resetState({ selectedSchema });
        }

        return this.schemasService.getSchemas(this.appName).pipe(
            tap(({ items, canCreate }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Schemas reloaded.');
                }

                return this.next(s => {
                    const schemas = ImmutableArray.of(items).sortByStringAsc(x => x.displayName);

                    return { ...s, schemas, isLoaded: true, canCreate };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateSchemaDto): Observable<SchemaDetailsDto> {
        return this.schemasService.postSchema(this.appName, request).pipe(
            tap(created => {
                this.next(s => {
                    const schemas = s.schemas.push(created).sortByStringAsc(x => x.displayName);

                    return { ...s, schemas };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public delete(schema: SchemaDto): Observable<any> {
        return this.schemasService.deleteSchema(this.appName, schema, schema.version).pipe(
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
            const categories = [...s.categories, name];

            return { ...s, categories: categories };
        });
    }

    public removeCategory(name: string) {
        this.next(s => {
            const categories = s.categories.filter(x => x !== name);

            return { ...s, categories: categories };
        });
    }

    public publish(schema: SchemaDto): Observable<SchemaDto> {
        return this.schemasService.publishSchema(this.appName, schema, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public unpublish(schema: SchemaDto): Observable<SchemaDto> {
        return this.schemasService.unpublishSchema(this.appName, schema, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public changeCategory(schema: SchemaDto, name: string): Observable<SchemaDto> {
        return this.schemasService.putCategory(this.appName, schema, { name }, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public configurePreviewUrls(schema: SchemaDto, request: {}): Observable<SchemaDetailsDto> {
        return this.schemasService.putPreviewUrls(this.appName, schema, request, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public configureScripts(schema: SchemaDto, request: {}): Observable<SchemaDetailsDto> {
        return this.schemasService.putScripts(this.appName, schema, request, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(schema: SchemaDto, request: UpdateSchemaDto): Observable<SchemaDetailsDto> {
        return this.schemasService.putSchema(this.appName, schema, request, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public addField(schema: SchemaDto, request: AddFieldDto, parent?: RootFieldDto): Observable<FieldDto> {
        return this.schemasService.postField(this.appName, parent || schema, request, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareMapSubscribed(this.dialogs, x => getField(x, request, parent), { silent: true }));
    }

    public orderFields(schema: SchemaDto, fields: any[], parent?: RootFieldDto): Observable<SchemaDetailsDto> {
        return this.schemasService.putFieldOrdering(this.appName, parent || schema, fields.map(t => t.fieldId), schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public lockField<T extends FieldDto>(schema: SchemaDto, field: T): Observable<SchemaDetailsDto> {
        return this.schemasService.lockField(this.appName, field, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public enableField<T extends FieldDto>(schema: SchemaDto, field: T): Observable<SchemaDetailsDto> {
        return this.schemasService.enableField(this.appName, field, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public disableField<T extends FieldDto>(schema: SchemaDto, field: T): Observable<SchemaDetailsDto> {
        return this.schemasService.disableField(this.appName, field, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public showField<T extends FieldDto>(schema: SchemaDto, field: T): Observable<SchemaDetailsDto> {
        return this.schemasService.showField(this.appName, field, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public hideField<T extends FieldDto>(schema: SchemaDto, field: T): Observable<SchemaDetailsDto> {
        return this.schemasService.hideField(this.appName, field, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public updateField<T extends FieldDto>(schema: SchemaDto, field: T, request: UpdateFieldDto): Observable<SchemaDetailsDto> {
        return this.schemasService.putField(this.appName, field, request, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteField(schema: SchemaDto, field: AnyFieldDto): Observable<SchemaDetailsDto> {
        return this.schemasService.deleteField(this.appName, field, schema.version).pipe(
            tap(updated => {
                this.replaceSchema(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceSchema(schema: SchemaDto) {
        return this.next(s => {
            const schemas = s.schemas.replaceBy('id', schema).sortByStringAsc(x => x.displayName);

            const selectedSchema =
                Types.is(schema, SchemaDetailsDto) &&
                schema &&
                s.selectedSchema &&
                s.selectedSchema.id === schema.id ?
                schema :
                s.selectedSchema;

            return { ...s, schemas, selectedSchema };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }
}

function getField(x: SchemaDetailsDto, request: AddFieldDto, parent?: RootFieldDto): FieldDto {
    if (parent) {
        return x.fields.find(f => f.fieldId === parent.fieldId)!.nested.find(f => f.name === request.name)!;
    } else {
        return x.fields.find(f => f.name === request.name)!;
    }
}

function buildCategories(categories: string[], schemas: SchemasList): SchemaCategory[] {
    const uniqueCategories: { [name: string]: string } = {};

    for (let category of categories) {
        uniqueCategories[category] = category;
    }

    for (let schema of schemas.values) {
        uniqueCategories[schema.category || 'Schemas'] = schema.category;
    }

    const result: SchemaCategory[] = [];

    for (let name in uniqueCategories) {
        if (uniqueCategories.hasOwnProperty(name)) {
            const key = uniqueCategories[name];

            result.push({ name, upper: name.toUpperCase(), schemas: schemas.filter(x => isSameCategory(key, x))});
        }
    }

    result.sort((a, b) => compareStringsAsc(a.upper, b.upper));

    return result;
}

export function isSameCategory(name: string, schema: SchemaDto): boolean {
    return (!name && !schema.category) || schema.category === name;
}