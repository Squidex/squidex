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
    AddFieldDto,
    AppsState,
    AuthService,
    createProperties,
    CreateSchemaDto,
    DateTime,
    DialogService,
    FieldDto,
    Form,
    ImmutableArray,
    SchemaDto,
    SchemaDetailsDto,
    SchemasService,
    State,
    UpdateFieldDto,
    UpdateSchemaScriptsDto,
    UpdateSchemaDto,
    ValidatorsEx,
    Version,
    SchemaPropertiesDto,
    FieldPropertiesDto
} from '@app/shared';

const FALLBACK_NAME = 'my-schema';

export class CreateForm extends Form<FormGroup> {
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
    schemas: ImmutableArray<SchemaDto>;
    selectedSchema?: SchemaDetailsDto | null;
}

@Injectable()
export class SchemasState extends State<Snapshot> {
    public selectedSchema =
        this.changes.map(s => s.selectedSchema)
            .distinctUntilChanged();

    public schemas =
        this.changes.map(s => s.schemas)
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
        super({ schemas: ImmutableArray.of() });
    }

    public selectSchema(id: string | null): Observable<SchemaDetailsDto | null> {
        return this.loadSchema(id)
            .do(schema => {
                this.next(s => {
                    const schemas = schema ? s.schemas.replaceBy('id', schema) : s.schemas;

                    return { ...s, selectedSchema: schema, schemas };
                });
            });
    }

    private loadSchema(id: string | null) {
        return !id ?
            Observable.of(null) :
            Observable.of(<SchemaDetailsDto>this.snapshot.schemas.find(x => x.id === id && x instanceof SchemaDetailsDto))
                .switchMap(schema => {
                    if (!schema) {
                        return this.schemasService.getSchema(this.appName, id).catch(() => Observable.of(null));
                    } else {
                        return Observable.of(schema);
                    }
                });
    }

    public load(): Observable<any> {
        return this.schemasService.getSchemas(this.appName)
            .do(dtos => {
                return this.next(s => {
                    const schemas = ImmutableArray.of(dtos).sortByStringAsc(x => x.displayName);

                    return { ...s, schemas };
                });
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public delete(schema: SchemaDto): Observable<any> {
        return this.schemasService.deleteSchema(this.appName, schema.name, schema.version)
            .do(dto => {
                return this.next(s => {
                    const schemas = s.schemas.filter(x => x.id !== schema.id);

                    return { ...s, schemas };
                });
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public create(request: CreateSchemaDto) {
        return this.schemasService.postSchema(this.appName, request, this.user, DateTime.now())
            .do(dto => {
                return this.next(s => {
                    const schemas = s.schemas.push(dto).sortByStringAsc(x => x.displayName);

                    return { ...s, schemas };
                });
            });
    }

    public addField(schema: SchemaDetailsDto, request: AddFieldDto): Observable<FieldDto> {
        return this.schemasService.postField(this.appName, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(addField(schema, dto.payload, this.user, dto.version));
            }).map(d => d.payload);
    }

    public publish(schema: SchemaDto): Observable<any> {
        return this.schemasService.publishSchema(this.appName, schema.name, schema.version)
            .do(dto => {
                this.replaceSchema(setPublished(schema, true, this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public unpublish(schema: SchemaDto): Observable<any> {
        return this.schemasService.unpublishSchema(this.appName, schema.name, schema.version)
            .do(dto => {
                this.replaceSchema(setPublished(schema, false, this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public enableField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.enableField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, setDisabled(field, false), this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public disableField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.disableField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, setDisabled(field, true), this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public lockField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.lockField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, setLocked(field, true), this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public showField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.showField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, setHidden(field, false), this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public hideField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.hideField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, setHidden(field, true), this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public deleteField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.deleteField(this.appName, schema.name, field.fieldId, schema.version)
            .do(dto => {
                this.replaceSchema(removeField(schema, field, this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public sortFields(schema: SchemaDetailsDto, fields: FieldDto[]): Observable<any> {
        return this.schemasService.putFieldOrdering(this.appName, schema.name, fields.map(t => t.fieldId), schema.version)
            .do(dto => {
                this.replaceSchema(replaceFields(schema, fields, this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public updateField(schema: SchemaDetailsDto, field: FieldDto, request: UpdateFieldDto): Observable<any> {
        return this.schemasService.putField(this.appName, schema.name, field.fieldId, request, schema.version)
            .do(dto => {
                this.replaceSchema(updateField(schema, update(field, request.properties), this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public configureScripts(schema: SchemaDetailsDto, request: UpdateSchemaScriptsDto): Observable<any> {
        return this.schemasService.putSchemaScripts(this.appName, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(configureScripts(schema, request, this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    public update(schema: SchemaDetailsDto, request: UpdateSchemaDto): Observable<any> {
        return this.schemasService.putSchema(this.appName, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(updateProperties(schema, request, this.user, dto.version));
            })
            .catch(error => this.dialogs.notifyError(error));
    }

    private replaceSchema(schema: SchemaDto) {
        return this.next(s => {
            const schemas = s.schemas.replaceBy('id', schema).sortByStringAsc(x => x.displayName);
            const selectedSchema = s.selectedSchema && s.selectedSchema.id === schema.id ? schema : s.selectedSchema;

            return { ...s, schemas, selectedSchema };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get user() {
        return this.authState.user!.token;
    }
}

const setPublished = (schema: SchemaDto | SchemaDetailsDto, publish: boolean, user: string, version: Version, now?: DateTime) => {
    if (schema instanceof SchemaDetailsDto) {
        return new SchemaDetailsDto(
            schema.id,
            schema.name,
            schema.properties,
            publish,
            schema.createdBy, user,
            schema.created, now || DateTime.now(),
            version,
            schema.fields,
            schema.scriptQuery,
            schema.scriptCreate,
            schema.scriptUpdate,
            schema.scriptDelete,
            schema.scriptChange);
    } else {
        return new SchemaDto(
            schema.id,
            schema.name,
            schema.properties,
            publish,
            schema.createdBy, user,
            schema.created, now || DateTime.now(),
            version);
    }
};

const configureScripts = (schema: SchemaDetailsDto, scripts: UpdateSchemaScriptsDto, user: string, version: Version, now?: DateTime) =>
    new SchemaDetailsDto(
        schema.id,
        schema.name,
        schema.properties,
        schema.isPublished,
        schema.createdBy, user,
        schema.created, now || DateTime.now(),
        version,
        schema.fields,
        scripts.scriptQuery,
        scripts.scriptCreate,
        scripts.scriptUpdate,
        scripts.scriptDelete,
        scripts.scriptChange);

const updateProperties = (schema: SchemaDetailsDto, properties: SchemaPropertiesDto, user: string, version: Version, now?: DateTime) =>
    new SchemaDetailsDto(
        schema.id,
        schema.name,
        properties,
        schema.isPublished,
        schema.createdBy, user,
        schema.created, now || DateTime.now(),
        version,
        schema.fields,
        schema.scriptQuery,
        schema.scriptCreate,
        schema.scriptUpdate,
        schema.scriptDelete,
        schema.scriptChange);

const addField = (schema: SchemaDetailsDto, field: FieldDto, user: string, version: Version, now?: DateTime) =>
    new SchemaDetailsDto(
        schema.id,
        schema.name,
        schema.properties,
        schema.isPublished,
        schema.createdBy, user,
        schema.created, now || DateTime.now(),
        version,
        [...schema.fields, field],
        schema.scriptQuery,
        schema.scriptCreate,
        schema.scriptUpdate,
        schema.scriptDelete,
        schema.scriptChange);

const updateField = (schema: SchemaDetailsDto, field: FieldDto, user: string, version: Version, now?: DateTime) =>
    new SchemaDetailsDto(
        schema.id,
        schema.name,
        schema.properties,
        schema.isPublished,
        schema.createdBy, user,
        schema.created, now || DateTime.now(),
        version,
        schema.fields.map(f => f.fieldId === field.fieldId ? field : f),
        schema.scriptQuery,
        schema.scriptCreate,
        schema.scriptUpdate,
        schema.scriptDelete,
        schema.scriptChange);

const replaceFields = (schema: SchemaDetailsDto, fields: FieldDto[], user: string, version: Version, now?: DateTime) =>
    new SchemaDetailsDto(
        schema.id,
        schema.name,
        schema.properties,
        schema.isPublished,
        schema.createdBy, user,
        schema.created, now || DateTime.now(),
        version,
        fields,
        schema.scriptQuery,
        schema.scriptCreate,
        schema.scriptUpdate,
        schema.scriptDelete,
        schema.scriptChange);

const removeField = (schema: SchemaDetailsDto, field: FieldDto, user: string, version: Version, now?: DateTime) =>
    new SchemaDetailsDto(
        schema.id,
        schema.name,
        schema.properties,
        schema.isPublished,
        schema.createdBy, user,
        schema.created, now || DateTime.now(),
        version,
        schema.fields.filter(f => f.fieldId !== field.fieldId),
        schema.scriptQuery,
        schema.scriptCreate,
        schema.scriptUpdate,
        schema.scriptDelete,
        schema.scriptChange);

const setLocked = (field: FieldDto, isLocked: boolean) =>
    new FieldDto(
        field.fieldId,
        field.name,
        isLocked,
        field.isHidden,
        field.isDisabled,
        field.partitioning,
        field.properties);

const setHidden = (field: FieldDto, isHidden: boolean) =>
    new FieldDto(
        field.fieldId,
        field.name,
        field.isLocked,
        isHidden,
        field.isDisabled,
        field.partitioning,
        field.properties);

const setDisabled = (field: FieldDto, isDisabled: boolean) =>
    new FieldDto(
        field.fieldId,
        field.name,
        field.isLocked,
        field.isDisabled,
        isDisabled,
        field.partitioning,
        field.properties);

const update = (field: FieldDto, properties: FieldPropertiesDto) =>
    new FieldDto(
        field.fieldId,
        field.name,
        field.isLocked,
        field.isHidden,
        field.isDisabled,
        field.partitioning,
        properties);