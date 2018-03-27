/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

import 'framework/utils/rxjs-extensions';

import {
    AddFieldDto,
    AppsState,
    AuthService,
    CreateSchemaDto,
    DateTime,
    DialogService,
    FieldDto,
    ImmutableArray,
    SchemaDto,
    SchemaDetailsDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaScriptsDto,
    UpdateSchemaDto
} from 'shared';

@Injectable()
export class SchemasState {
    public schemasItems = new BehaviorSubject<ImmutableArray<SchemaDto>>(ImmutableArray.empty());

    public selectedSchema = new BehaviorSubject<SchemaDetailsDto | null>(null);

    private get app() {
        return this.appsState.selectedApp.value!.name;
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

    public selectSchema(id: string | null): Observable<boolean> {
        const observable =
            !id ?
            Observable.of(null) :
            Observable.of(<SchemaDetailsDto>this.schemasItems.value.find(x => x.id === id && x instanceof SchemaDetailsDto))
                .switchMap(schema => {
                    if (!schema) {
                        return this.schemasService.getSchema(this.app, id).catch(() => Observable.of(null));
                    } else {
                        return Observable.of(schema);
                    }
                });

        return observable
            .do(schema => {
                this.selectedSchema.next(schema);
            })
            .map(s => s !== null);
    }

    public load(): Observable<any> {
        return this.schemasService.getSchemas(this.app)
            .catch(error => this.dialogs.notifyError(error))
            .do(dtos => {
                this.schemasItems.nextBy(v => ImmutableArray.of(dtos));
            });
    }

    public create(request: CreateSchemaDto) {
        return this.schemasService.postSchema(this.app, request, this.user, DateTime.now())
            .do(dto => {
                this.schemasItems.nextBy(v => v.push(dto));
            });
    }

    public addField(schema: SchemaDetailsDto, request: AddFieldDto): Observable<FieldDto> {
        return this.schemasService.postField(this.app, schema.name, request, schema.version)
            .do(dto => {
                this.replaceSchema(schema.addField(dto.payload, this.user, dto.version));
            }).map(d => d.payload);
    }

    public publish(schema: SchemaDto): Observable<any> {
        return this.schemasService.publishSchema(this.app, schema.name, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.publish(this.user, dto.version));
            });
    }

    public unpublish(schema: SchemaDto): Observable<any> {
        return this.schemasService.unpublishSchema(this.app, schema.name, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.unpublish(this.user, dto.version));
            });
    }

    public enableField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.enableField(this.app, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.enable(), this.user, dto.version));
            });
    }

    public disableField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.disableField(this.app, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.disable(), this.user, dto.version));
            });
    }

    public lockField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.lockField(this.app, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.lock(), this.user, dto.version));
            });
    }

    public showField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.showField(this.app, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.show(), this.user, dto.version));
            });
    }

    public hideField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.hideField(this.app, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.hide(), this.user, dto.version));
            });
    }

    public deleteField(schema: SchemaDetailsDto, field: FieldDto): Observable<any> {
        return this.schemasService.deleteField(this.app, schema.name, field.fieldId, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.removeField(field, this.user, dto.version));
            });
    }

    public sortFields(schema: SchemaDetailsDto, fields: FieldDto[]): Observable<any> {
        return this.schemasService.putFieldOrdering(this.app, schema.name, fields.map(t => t.fieldId), schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.replaceFields(fields, this.user, dto.version));
            });
    }

    public updateField(schema: SchemaDetailsDto, field: FieldDto, request: UpdateFieldDto): Observable<any> {
        return this.schemasService.putField(this.app, schema.name, field.fieldId, request, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.updateField(field.update(request.properties), this.user, dto.version));
            });
    }

    public configureScripts(schema: SchemaDetailsDto, request: UpdateSchemaScriptsDto): Observable<any> {
        return this.schemasService.putSchemaScripts(this.app, schema.name, request, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.configureScripts(request, this.user, dto.version));
            });
    }

    public update(schema: SchemaDetailsDto, request: UpdateSchemaDto): Observable<any> {
        return this.schemasService.putSchema(this.app, schema.name, request, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.replaceSchema(schema.update(request, this.user, dto.version));
            });
    }

    public delete(schema: SchemaDto): Observable<any> {
        return this.schemasService.deleteSchema(this.app, schema.name, schema.version)
            .catch(error => this.dialogs.notifyError(error))
            .do(dto => {
                this.schemasItems.nextBy(v => v.filter(s => s.id !== schema.id));
            });
    }

    private replaceSchema(schema: SchemaDto) {
        this.schemasItems.nextBy(v => v.replaceBy('id', schema));

        this.selectedSchema.nextBy(v => v !== null && v.id === schema.id ? <SchemaDetailsDto>schema : v);
    }

    public trackBySchema(index: number, schema: SchemaDto): any {
        return schema.id;
    }

    public trackByField(index: number, field: FieldDto): any {
        return field.fieldId;
    }
}