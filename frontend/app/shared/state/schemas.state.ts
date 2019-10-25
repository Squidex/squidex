/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { empty, Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import {
    compareStrings,
    defined,
    DialogService,
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
    categories: ReadonlyArray<string>;

    // The current schemas.
    schemas: SchemasList;

    // Indicates if the schemas are loaded.
    isLoaded?: boolean;

    // The selected schema.
    selectedSchema?: SchemaDetailsDto | null;

    // Indicates if the user can create a schema.
    canCreate?: boolean;
}

export type SchemasList = ReadonlyArray<SchemaDto>;
export type SchemaCategory = { name: string; schemas: SchemasList; upper: string; };

function sameSchema(lhs: SchemaDetailsDto | null, rhs?: SchemaDetailsDto | null): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id && lhs.version === rhs.version);
}

@Injectable()
export class SchemasState extends State<Snapshot> {
    public get schemaName() {
        return this.snapshot.selectedSchema ? this.snapshot.selectedSchema.name : '';
    }

    public categoriesPlain =
        this.project(x => x.categories);

    public selectedSchemaOrNull =
        this.project(x => x.selectedSchema, sameSchema);

    public selectedSchema =
        this.selectedSchemaOrNull.pipe(defined());

    public schemas =
        this.project(x => x.schemas);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public publishedSchemas =
        this.projectFrom(this.schemas, x => x.filter(s => s.isPublished));

    public categories =
        this.projectFrom2(this.schemas, this.categoriesPlain, (s, c) => buildCategories(c, s));

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly schemasService: SchemasService
    ) {
        super({ schemas: [], categories: [] });
    }

    public select(idOrName: string | null): Observable<SchemaDetailsDto | null> {
        return this.loadSchema(idOrName).pipe(
            tap(selectedSchema => {
                this.next(s => {
                    return { ...s, selectedSchema };
                });
            }));
    }

    public loadSchema(idOrName: string | null, cached = false) {
        if (!idOrName) {
            return of(null);
        }

        if (cached) {
            const found = this.snapshot.schemas.find(x => x.id === idOrName || x.name === idOrName);

            if (Types.is(found, SchemaDetailsDto)) {
                return of(found);
            }
        }

        return this.schemasService.getSchema(this.appName, idOrName).pipe(
            tap(schema => {
                this.next(s => {
                    const schemas = s.schemas.replaceBy('id', schema);

                    return { ...s, schemas };
                });
            }),
            catchError(() => of(null)));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState({ selectedSchema: this.snapshot.selectedSchema });
        }

        return this.loadInternal(isReload);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return empty();
        }

        return this.loadInternal(false);
    }

    private loadInternal(isReload = false): Observable<any> {
        return this.schemasService.getSchemas(this.appName).pipe(
            tap(({ items, canCreate }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Schemas reloaded.');
                }

                return this.next(s => {
                    const schemas = items.sortedByString(x => x.displayName);

                    return { ...s, schemas, isLoaded: true, canCreate };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateSchemaDto): Observable<SchemaDetailsDto> {
        return this.schemasService.postSchema(this.appName, request).pipe(
            tap(created => {
                this.next(s => {
                    const schemas = [...s.schemas, created].sortedByString(x => x.displayName);

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
            const categories = s.categories.removed(name);

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

    public synchronize(schema: SchemaDto, request: {}): Observable<SchemaDetailsDto> {
        return this.schemasService.putSchemaSync(this.appName, schema, request, schema.version).pipe(
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

    public orderFields(schema: SchemaDto, fields: ReadonlyArray<any>, parent?: RootFieldDto): Observable<SchemaDetailsDto> {
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
            const schemas = s.schemas.replaceBy('id', schema).sortedByString(x => x.displayName);

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

function buildCategories(categories: ReadonlyArray<string>, schemas: SchemasList): ReadonlyArray<SchemaCategory> {
    const uniqueCategories: { [name: string]: true } = {};

    for (const category of categories) {
        uniqueCategories[category] = true;
    }

    for (const schema of schemas) {
        uniqueCategories[getCategory(schema)] = true;
    }

    const result: SchemaCategory[] = [];

    for (const name in uniqueCategories) {
        if (uniqueCategories.hasOwnProperty(name)) {
            result.push({ name, upper: name.toUpperCase(), schemas: schemas.filter(x => isSameCategory(name, x))});
        }
    }

    result.sort((a, b) => compareStrings(a.upper, b.upper));

    return result;
}

function getCategory(schema: SchemaDto) {
    return schema.category || 'Schemas';
}

export function isSameCategory(name: string, schema: SchemaDto): boolean {
    return getCategory(schema) === name;
}