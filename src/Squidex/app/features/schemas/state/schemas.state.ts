/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';

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
    UpdateFieldDto,
    UpdateSchemaScriptsDto,
    UpdateSchemaDto,
    ValidatorsEx
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

@Injectable()
export class SchemasState {
    public schemasItems = new BehaviorSubject<ImmutableArray<SchemaDto>>(ImmutableArray.empty());

    public selectedSchema = new BehaviorSubject<SchemaDetailsDto | null>(null);
    public selectedReferencedSchema = new BehaviorSubject<SchemaDetailsDto | null>(null);

    public get schemaName() {
        return this.selectedSchema.value!.name;
    }

    public get referencedSchemaName() {
        return this.selectedReferencedSchema.value!.name;
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get user() {
        return this.authState.user!.token;
    }

    constructor(
        private readonly schemasService: SchemasService,
        private readonly dialogs: DialogService,
        private readonly authState: AuthService,
        private readonly appsState: AppsState
    ) {
    }

    public selectSchema(id: string | null): Observable<SchemaDetailsDto | null> {
        return this.loadSchema(id)
            .do(schema => {
                this.selectedSchema.next(schema);
            });
    }

    public selectReferencedSchema(id: string | null): Observable<SchemaDetailsDto | null> {
        return this.loadSchema(id)
            .do(schema => {
                this.selectedReferencedSchema.next(schema);
            });
    }

    private loadSchema(id: string | null) {
        return !id ?
            Observable.of(null) :
            Observable.of(<SchemaDetailsDto>this.schemasItems.value.find(x => x.id === id && x instanceof SchemaDetailsDto))
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
            .catch(error => this.dialogs.notifyError(error))
            .do(dtos => {
                this.schemasItems.nextBy(v => ImmutableArray.of(dtos));
            });
    }

    public create(request: CreateSchemaDto) {
        return this.schemasService.postSchema(this.appName, request, this.user, DateTime.now())
            .do(dto => {
                this.schemasItems.nextBy(v => v.push(dto));
            });
    }

    public addField(schema: SchemaDetailsDto, request: AddFieldDto): Observable<FieldDto> {
        return this.schemasService.postField(this.appName, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(schema.addField(dto.payload, this.user, dto.version));
            }).map(d => d.payload);
    }

    public publish(schema: SchemaDto): Observable<any> {
        return this.schemasService.publishSchema(this.appName, schema.name, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.publish(this.user, dto.version));
            });
    }

    public unpublish(schema: SchemaDto): Observable<any> {
        return this.schemasService.unpublishSchema(this.appName, schema.name, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.unpublish(this.user, dto.version));
            });
    }

    public enableField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.enableField(this.appName, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.enable(), this.user, dto.version));
            });
    }

    public disableField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.disableField(this.appName, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.disable(), this.user, dto.version));
            });
    }

    public lockField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.lockField(this.appName, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.lock(), this.user, dto.version));
            });
    }

    public showField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.showField(this.appName, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.show(), this.user, dto.version));
            });
    }

    public hideField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.hideField(this.appName, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.hide(), this.user, dto.version));
            });
    }

    public deleteField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.deleteField(this.appName, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.removeField(field, this.user, dto.version));
            });
    }

    public sortFields(schema: SchemaDetailsDto, fields: FieldDto[]): Observable<any> {
        return this.schemasService.putFieldOrdering(this.appName, schema.name, fields.map(t => t.fieldId), schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.replaceFields(fields, this.user, dto.version));
            });
    }

    public updateField(schema: SchemaDetailsDto, field: FieldDto, request: UpdateFieldDto): Observable<any> {
        return this.schemasService.putField(this.appName, schema.name, field.fieldId, request, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.update(request.properties), this.user, dto.version));
            });
    }

    public configureScripts(schema: SchemaDetailsDto, request: UpdateSchemaScriptsDto): Observable<any> {
        return this.schemasService.putSchemaScripts(this.appName, schema.name, request, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.configureScripts(request, this.user, dto.version));
            });
    }

    public update(schema: SchemaDetailsDto, request: UpdateSchemaDto): Observable<any> {
        return this.schemasService.putSchema(this.appName, schema.name, request, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.update(request, this.user, dto.version));
            });
    }

    public delete(schema: SchemaDto): Observable<any> {
        return this.schemasService.deleteSchema(this.appName, schema.name, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.schemasItems.nextBy(v => v.filter(s => s.id !== schema.id));
            });
    }

    private replaceSchema(schema: SchemaDto) {
        this.schemasItems.nextBy(v => v.replaceBy('id', schema));

        if (schema instanceof SchemaDetailsDto) {
            this.selectedSchema.nextBy(v => v !== null && v.id === schema.id ? schema : v);
            this.selectedReferencedSchema.nextBy(v => v !== null && v.id === schema.id ? schema : v);
        }
    }

    public trackBySchema(index: number, schema: SchemaDto): any {
        return schema.id;
    }

    public trackByField(index: number, field: FieldDto): any {
        return field.fieldId;
    }
}