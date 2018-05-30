/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DateTime,
    DialogService,
    Form,
    ImmutableArray,
    State,
    Types,
    ValidatorsEx,
    Version
} from '@app/framework';

import { AuthService } from './../services/auth.service';
import { AppsState } from './apps.state';

import {
    AddFieldDto,
    AnyFieldDto,
    createProperties,
    CreateSchemaDto,
    FieldDto,
    FieldPropertiesDto,
    NestedFieldDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaCategoryDto,
    UpdateSchemaDto,
    UpdateSchemaScriptsDto
} from './../services/schemas.service';

const FALLBACK_NAME = 'my-schema';

export class CreateCategoryForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['']
        }));
    }
}

export class CreateSchemaForm extends Form<FormGroup> {
    public schemaName =
        this.form.controls['name'].valueChanges.map(n => n || FALLBACK_NAME)
            .startWith(FALLBACK_NAME);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes only (not at the end).')
                ]
            ],
            import: {}
        }));
    }
}

export class EditScriptsForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            scriptQuery: '',
            scriptCreate: '',
            scriptUpdate: '',
            scriptDelete: '',
            scriptChange: ''
        }));
    }
}

export class EditFieldForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100)
                ]
            ],
            hints: ['',
                [
                    Validators.maxLength(1000)
                ]
            ],
            placeholder: ['',
                [
                    Validators.maxLength(1000)
                ]
            ],
            editorUrl: null,
            isRequired: false,
            isListField: false
        }));
    }
}

export class EditSchemaForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100)
                ]
            ],
            hints: ['',
                [
                    Validators.maxLength(1000)
                ]
            ]
        }));
    }
}

export class AddFieldForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            type: ['String',
                [
                    Validators.required
                ]
            ],
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'Name must be a valid javascript name in camel case.')
                ]
            ],
            isLocalizable: false
        }));
    }

    public submit() {
        const value = super.submit();

        if (value) {
            const properties = createProperties(value.type);
            const partitioning = value.isLocalizable ? 'language' : 'invariant';

            return { name: value.name, partitioning, properties };
        }

        return null;
    }
}

interface Snapshot {
    categories: { [name: string]: boolean };

    schemasApp?: string;
    schemas: ImmutableArray<SchemaDto>;

    isLoaded?: boolean;

    selectedSchema?: SchemaDetailsDto | null;
}

@Injectable()
export class SchemasState extends State<Snapshot> {
    public selectedSchema =
        this.changes.map(x => x.selectedSchema)
            .distinctUntilChanged();

    public categories =
        this.changes.map(x => ImmutableArray.of(Object.keys(x.categories)).sortByStringAsc(s => s))
            .distinctUntilChanged();

    public schemas =
        this.changes.map(x => x.schemas)
            .distinctUntilChanged();

    public publishedSchemas =
        this.changes.map(x => x.schemas.filter(s => s.isPublished))
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

    public get schemaName() {
        return this.snapshot.selectedSchema!.name;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly schemasService: SchemasService
    ) {
        super({ schemas: ImmutableArray.empty(), categories: {} });
    }

    public select(idOrName: string | null): Observable<SchemaDetailsDto | null> {
        return this.loadSchema(idOrName)
            .do(schema => {
                this.next(s => {
                    const schemas = schema ? s.schemas.replaceBy('id', schema) : s.schemas;

                    return { ...s, selectedSchema: schema, schemas };
                });
            });
    }

    private loadSchema(idOrName: string | null) {
        return !idOrName ? Observable.of(null) :
            this.schemasService.getSchema(this.appName, idOrName)
                .catch(() => Observable.of(null));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.schemasService.getSchemas(this.appName)
            .do(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Schemas reloaded.');
                }

                return this.next(s => {
                    const schemas = ImmutableArray.of(dtos).sortByStringAsc(x => x.displayName);

                    const categories = buildCategories(s.categories, schemas);

                    return { ...s, schemas, schemasApp: this.appName, isLoaded: true, categories };
                });
            })
            .notify(this.dialogs);
    }

    public create(request: CreateSchemaDto, now?: DateTime) {
        return this.schemasService.postSchema(this.appName, request, this.user, now || DateTime.now())
            .do(dto => {
                return this.next(s => {
                    const schemas = s.schemas.push(dto).sortByStringAsc(x => x.displayName);

                    return { ...s, schemas };
                });
            });
    }

    public delete(schema: SchemaDto): Observable<any> {
        return this.schemasService.deleteSchema(this.appName, schema.name, schema.version)
            .do(dto => {
                return this.next(s => {
                    const schemas = s.schemas.filter(x => x.id !== schema.id);
                    const selectedSchema = s.selectedSchema && s.selectedSchema.id === schema.id ? null : s.selectedSchema;

                    return { ...s, schemas, selectedSchema };
                });
            })
            .notify(this.dialogs);
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

    public publish(schema: SchemaDto, now?: DateTime): Observable<any> {
        return this.schemasService.publishSchema(this.appName, schema.name, schema.version)
            .do(dto => {
                this.replaceSchema(setPublished(schema, true, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public unpublish(schema: SchemaDto, now?: DateTime): Observable<any> {
        return this.schemasService.unpublishSchema(this.appName, schema.name, schema.version)
            .do(dto => {
                this.replaceSchema(setPublished(schema, false, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public changeCategory(schema: SchemaDto, name: string, now?: DateTime): Observable<any> {
        return this.schemasService.putCategory(this.appName, schema.name, new UpdateSchemaCategoryDto(name), schema.version)
            .do(dto => {
                this.replaceSchema(changeCategory(schema, name, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public configureScripts(schema: SchemaDetailsDto, request: UpdateSchemaScriptsDto, now?: DateTime): Observable<any> {
        return this.schemasService.putScripts(this.appName, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(configureScripts(schema, request, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public update(schema: SchemaDetailsDto, request: UpdateSchemaDto, now?: DateTime): Observable<any> {
        return this.schemasService.putSchema(this.appName, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(updateProperties(schema, request, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public lockField(schema: SchemaDetailsDto, field: RootFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.lockField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, setLocked(field, true), this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public addField(schema: SchemaDetailsDto, request: AddFieldDto, parent?: RootFieldDto, now?: DateTime): Observable<FieldDto> {
        return this.schemasService.postField(this.appName, schema.name, request, pid(parent), schema.version)
            .do(dto => {
                if (Types.is(dto.payload, NestedFieldDto)) {
                    this.replaceSchema(updateField(schema, addNested(parent!, dto.payload), this.user, dto.version, now));
                } else {
                    this.replaceSchema(addField(schema, dto.payload, this.user, dto.version, now));
                }
            }).map(d => d.payload);
    }

    public sortFields(schema: SchemaDetailsDto, fields: any[], parent?: RootFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.putFieldOrdering(this.appName, schema.name, fields.map(t => t.fieldId), pid(parent), schema.version)
            .do(dto => {
                if (!parent) {
                    this.replaceSchema(replaceFields(schema, fields, this.user, dto.version, now));
                } else {
                    this.replaceSchema(updateField(schema, replaceNested(parent, fields), this.user, dto.version, now));
                }
            })
            .notify(this.dialogs);
    }

    public enableField(schema: SchemaDetailsDto, field: AnyFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.enableField(this.appName, schema.name, field.fieldId, pidof(field), schema.version)
            .do(dto => {
                this.replaceField(schema, setDisabled(field, false), dto.version, now);
            })
            .notify(this.dialogs);
    }

    public disableField(schema: SchemaDetailsDto, field: AnyFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.disableField(this.appName, schema.name, field.fieldId, pidof(field), schema.version)
            .do(dto => {
                this.replaceField(schema, setDisabled(field, true), dto.version, now);
            })
            .notify(this.dialogs);
    }

    public showField(schema: SchemaDetailsDto, field: AnyFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.showField(this.appName, schema.name, field.fieldId, pidof(field), schema.version)
            .do(dto => {
                this.replaceField(schema, setHidden(field, false), dto.version, now);
            })
            .notify(this.dialogs);
    }

    public hideField(schema: SchemaDetailsDto, field: AnyFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.hideField(this.appName, schema.name, field.fieldId, pidof(field), schema.version)
            .do(dto => {
                this.replaceField(schema, setHidden(field, true), dto.version, now);
            })
            .notify(this.dialogs);
    }

    public updateField(schema: SchemaDetailsDto, field: AnyFieldDto, request: UpdateFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.putField(this.appName, schema.name, field.fieldId, request, pidof(field), schema.version)
            .do(dto => {
                this.replaceField(schema, update(field, request.properties), dto.version, now);
            })
            .notify(this.dialogs);
    }

    public deleteField(schema: SchemaDetailsDto, field: AnyFieldDto, now?: DateTime): Observable<any> {
        return this.schemasService.deleteField(this.appName, schema.name, field.fieldId, pidof(field), schema.version)
            .do(dto => {
                this.removeField(schema, field, dto.version, now);
            })
            .notify(this.dialogs);
    }

    private replaceField(schema: SchemaDetailsDto, field: AnyFieldDto, version: Version, now?: DateTime) {
        if (Types.is(field, RootFieldDto)) {
            this.replaceSchema(updateField(schema, field, this.user, version, now));
        } else {
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
            const selectedSchema = s.selectedSchema && s.selectedSchema.id === schema.id ? schema : s.selectedSchema;

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

function buildCategories(categories: { [name: string]: boolean }, schemas: ImmutableArray<SchemaDto>) {
    categories = { ...categories };

    for (let category in categories) {
        if (!categories[category]) {
            delete categories[category];
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

const configureScripts = (schema: SchemaDetailsDto, scripts: UpdateSchemaScriptsDto, user: string, version: Version, now?: DateTime) =>
    schema.with({
        ...scripts,
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

const setLocked = (field: RootFieldDto, isLocked: boolean) =>
    field.with({ isLocked });

const setHidden = <T extends FieldDto>(field: T, isHidden: boolean) =>
    <T>field.with(<any>{ isHidden });

const setDisabled = <T extends FieldDto>(field: T, isDisabled: boolean) =>
    <T>field.with(<any>{ isDisabled });

const update = <T extends FieldDto>(field: T, properties: FieldPropertiesDto) =>
    <T>field.with({ properties });

const pid = (field?: RootFieldDto) =>
    field ? field.fieldId : undefined;

const pidof = (field: FieldDto) =>
    Types.is(field, NestedFieldDto) ? field.parentId : undefined;