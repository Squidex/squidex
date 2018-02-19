/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    AddFieldDto,
    AppContext,
    AppPatternDto,
    AppPatternsService,
    createProperties,
    fadeAnimation,
    FieldDto,
    FieldPropertiesDto,
    fieldTypes,
    HistoryChannelUpdated,
    ImmutableArray,
    ModalView,
    SchemaDetailsDto,
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    UpdateSchemaScriptsDto,
    ValidatorsEx,
    Version
} from 'shared';

import {
    SchemaCloning,
    SchemaCreated,
    SchemaDeleted,
    SchemaUpdated
} from './../messages';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class SchemaPageComponent implements OnDestroy, OnInit {
    private schemaCreatedSubscription: Subscription;

    public fieldTypes = fieldTypes;

    public schemaExport: any;
    public schema: SchemaDetailsDto;
    public schemas: ImmutableArray<SchemaDto>;

    public regexSuggestions: AppPatternDto[] = [];

    public exportSchemaDialog = new ModalView();

    public configureScriptsDialog = new ModalView();

    public editOptionsDropdown = new ModalView();
    public editSchemaDialog = new ModalView();

    public addFieldFormSubmitted = false;
    public addFieldForm =
        this.formBuilder.group({
            type: ['String',
                [
                    Validators.required
                ]],
            name: ['',
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'Name must be a valid javascript name in camel case.')
                ]],
            isLocalizable: [false]
        });

    public get hasName() {
        return this.addFieldForm.controls['name'].value && this.addFieldForm.controls['name'].value.length > 0;
    }

    constructor(public readonly ctx: AppContext,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router,
        private readonly schemasService: SchemasService,
        private readonly appPatternsService: AppPatternsService
    ) {
    }

    public ngOnDestroy() {
        this.schemaCreatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.schemaCreatedSubscription =
            this.ctx.bus.of(SchemaCreated)
                .subscribe(message => {
                    if (this.schemas) {
                        this.schemas = this.schemas.push(message.schema);
                    }
                });

        this.ctx.route.data.map(d => d.schema)
            .subscribe((schema: SchemaDetailsDto) => {
                this.schema = schema;

                this.export();
            });

        this.load();
    }

    private load() {
        this.appPatternsService.getPatterns(this.ctx.appName)
            .subscribe(dtos => {
                this.regexSuggestions = dtos.patterns;
            });

        this.schemasService.getSchemas(this.ctx.appName)
            .subscribe(dtos => {
                this.schemas = ImmutableArray.of(dtos);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public publish() {
        this.schemasService.publishSchema(this.ctx.appName, this.schema.name, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.publish(this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public unpublish() {
        this.schemasService.unpublishSchema(this.ctx.appName, this.schema.name, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.unpublish(this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public enableField(field: FieldDto) {
        this.schemasService.enableField(this.ctx.appName, this.schema.name, field.fieldId, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.updateField(field.enable(), this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public disableField(field: FieldDto) {
        this.schemasService.disableField(this.ctx.appName, this.schema.name, field.fieldId, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.updateField(field.disable(), this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public lockField(field: FieldDto) {
        this.schemasService.lockField(this.ctx.appName, this.schema.name, field.fieldId, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.updateField(field.lock(), this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public showField(field: FieldDto) {
        this.schemasService.showField(this.ctx.appName, this.schema.name, field.fieldId, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.updateField(field.show(), this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public hideField(field: FieldDto) {
        this.schemasService.hideField(this.ctx.appName, this.schema.name, field.fieldId, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.updateField(field.hide(), this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public deleteField(field: FieldDto) {
        this.schemasService.deleteField(this.ctx.appName, this.schema.name, field.fieldId, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.removeField(field, this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public sortFields(fields: FieldDto[]) {
        this.schemasService.putFieldOrdering(this.ctx.appName, this.schema.name, fields.map(t => t.fieldId), this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.replaceFields(fields, this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public saveField(field: FieldDto, properties: FieldPropertiesDto) {
        const requestDto = new UpdateFieldDto(properties);

        this.schemasService.putField(this.ctx.appName, this.schema.name, field.fieldId, requestDto, this.schema.version)
            .subscribe(dto => {
                this.updateSchema(this.schema.updateField(field.update(properties), this.ctx.userToken, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public deleteSchema() {
        this.schemasService.deleteSchema(this.ctx.appName, this.schema.name, this.schema.version)
            .subscribe(() => {
                this.onSchemaRemoved(this.schema);
                this.back();
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public addField() {
        this.addFieldFormSubmitted = true;

        if (this.addFieldForm.valid) {
            this.addFieldForm.disable();

            const properties = createProperties(this.addFieldForm.controls['type'].value);
            const partitioning = this.addFieldForm.controls['isLocalizable'].value ? 'language' : 'invariant';

            const requestDto = new AddFieldDto(this.addFieldForm.controls['name'].value, partitioning, properties);

            this.schemasService.postField(this.ctx.appName, this.schema.name, requestDto, this.schema.version)
                .subscribe(dto => {
                    this.updateSchema(this.schema.addField(dto.payload, this.ctx.userToken, dto.version));
                    this.resetFieldForm();
                }, error => {
                    this.ctx.notifyError(error);
                    this.resetFieldForm();
                });
        }
    }

    public cancelAddField() {
        this.resetFieldForm();
    }

    public cloneSchema() {
        this.ctx.bus.emit(new SchemaCloning(this.schemaExport));
    }

    public onSchemaSaved(properties: SchemaPropertiesDto, version: Version) {
        this.updateSchema(this.schema.update(properties, this.ctx.userToken, version));

        this.editSchemaDialog.hide();
    }

    public onSchemaScriptsSaved(scripts: UpdateSchemaScriptsDto, version: Version) {
        this.updateSchema(this.schema.configureScripts(scripts, this.ctx.userToken, version));

        this.configureScriptsDialog.hide();
    }

    private onSchemaRemoved(schema: SchemaDto) {
        this.schemas = this.schemas.removeAll(s => s.id === schema.id);

        this.emitSchemaDeleted(schema);
    }

    private resetFieldForm() {
        this.addFieldForm.enable();
        this.addFieldForm.reset({ type: 'String' });
        this.addFieldFormSubmitted = false;
    }

    private updateSchema(schema: SchemaDetailsDto) {
        this.schema = schema;
        this.schemas = this.schemas.replaceBy('id', schema);

        this.emitSchemaUpdated(schema);
        this.emitHistoryUpdate();
        this.export();
    }

    private export() {
        const result: any = {
            fields: this.schema.fields.map(field => {
                const copy: any = Object.assign({}, field);

                delete copy.fieldId;

                for (const key in copy.properties) {
                    if (copy.properties.hasOwnProperty(key)) {
                        if (!copy.properties[key]) {
                            delete copy.properties[key];
                        }
                    }
                }

                return copy;
            }),
            properties: {}
        };

        if (this.schema.properties.label) {
            result.properties.label = this.schema.properties.label;
        }

        if (this.schema.properties.hints) {
            result.properties.hints = this.schema.properties.hints;
        }

        this.schemaExport = result;
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.ctx.route });
    }

    private emitSchemaDeleted(schema: SchemaDto) {
        this.ctx.bus.emit(new SchemaDeleted(schema));
    }

    private emitSchemaUpdated(schema: SchemaDto) {
        this.ctx.bus.emit(new SchemaUpdated(schema));
    }

    private emitHistoryUpdate() {
        this.ctx.bus.emit(new HistoryChannelUpdated());
    }
}

