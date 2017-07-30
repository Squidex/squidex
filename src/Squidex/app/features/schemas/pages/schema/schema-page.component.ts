/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import {
    AddFieldDto,
    AppComponentBase,
    AppsStoreService,
    AuthService,
    createProperties,
    fadeAnimation,
    FieldDto,
    fieldTypes,
    HistoryChannelUpdated,
    MessageBus,
    ModalView,
    NotificationService,
    SchemaDetailsDto,
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    ValidatorsEx
} from 'shared';

import { SchemaDeleted, SchemaUpdated } from './../messages';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaPageComponent extends AppComponentBase implements OnInit {
    public fieldTypes = fieldTypes;

    public schemaExport: any;
    public schema: SchemaDetailsDto;
    public schemas: SchemaDto[];

    public confirmDeleteDialog = new ModalView();

    public exportSchemaDialog = new ModalView();

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
                    ValidatorsEx.pattern('[a-z0-9]+(\\-[a-zA-Z0-9]+)*', 'Name must be a valid javascript name in camel case.')
                ]],
            isLocalizable: [false]
        });

    public get hasName() {
        return this.addFieldForm.controls['name'].value && this.addFieldForm.controls['name'].value.length > 0;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly authService: AuthService,
        private readonly formBuilder: FormBuilder,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasService: SchemasService
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        this.route.data.map(p => p['schema'])
            .subscribe((schema: SchemaDetailsDto) => {
                this.schema = schema;

                this.export();
            });

        this.load();
    }

    private load() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.getSchemas(app))
            .subscribe(dtos => {
                this.schemas = dtos;
            }, error => {
                this.notifyError(error);
            });
    }

    public publish() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.publishSchema(app, this.schema.name, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.publish(this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public unpublish() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.unpublishSchema(app, this.schema.name, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.unpublish(this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public enableField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.enableField(app, this.schema.name, field.fieldId, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.updateField(field.enable(), this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public disableField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.disableField(app, this.schema.name, field.fieldId, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.updateField(field.disable(), this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public showField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.showField(app, this.schema.name, field.fieldId, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.updateField(field.show(), this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public hideField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.hideField(app, this.schema.name, field.fieldId, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.updateField(field.hide(), this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.deleteField(app, this.schema.name, field.fieldId, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.removeField(field, this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public sortFields(fields: FieldDto[]) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.putFieldOrdering(app, this.schema.name, fields.map(t => t.fieldId), this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.replaceFields(fields, this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public saveField(field: FieldDto) {
        const requestDto = new UpdateFieldDto(field.properties);

        this.appNameOnce()
            .switchMap(app => this.schemasService.putField(app, this.schema.name, field.fieldId, requestDto, this.schema.version)).retry(2)
            .subscribe(() => {
                this.updateSchema(this.schema.updateField(field, this.authService.user.token));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteSchema() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.deleteSchema(app, this.schema.name, this.schema.version)).retry(2)
            .subscribe(() => {
                this.emitSchemaDeleted(this.schema);
                this.hideDeleteDialog();
                this.back();
            }, error => {
                this.hideDeleteDialog();
                this.notifyError(error);
            });
    }

    public addField() {
        this.addFieldFormSubmitted = true;

        if (this.addFieldForm.valid) {
            this.addFieldForm.disable();

            const properties = createProperties(this.addFieldForm.controls['type'].value);
            const partitioning = this.addFieldForm.controls['isLocalizable'].value ? 'language' : 'invariant';

            const requestDto = new AddFieldDto(this.addFieldForm.controls['name'].value, partitioning, properties);

            this.appNameOnce()
                .switchMap(app => this.schemasService.postField(app, this.schema.name, requestDto, this.schema.version))
                .subscribe(dto => {
                    this.updateSchema(this.schema.addField(dto, this.authService.user.token));
                    this.resetFieldForm();
                }, error => {
                    this.notifyError(error);
                    this.resetFieldForm();
                });
        }
    }

    public cancelAddField() {
        this.resetFieldForm();
    }

    public onSchemaSaved(properties: SchemaPropertiesDto) {
        this.updateSchema(this.schema.update(properties, this.authService.user.token));

        this.editSchemaDialog.hide();
    }

    private resetFieldForm() {
        this.addFieldForm.enable();
        this.addFieldForm.reset({ type: 'String' });
        this.addFieldFormSubmitted = false;
    }

    private updateSchema(schema: SchemaDetailsDto) {
        this.schema = schema;

        this.emitSchemaUpdated(schema);
        this.notify();
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
        this.router.navigate(['../'], { relativeTo: this.route });
    }

    private emitSchemaDeleted(schema: SchemaDto) {
        this.messageBus.emit(new SchemaDeleted(schema));
    }

    private emitSchemaUpdated(schema: SchemaDto) {
        this.messageBus.emit(new SchemaUpdated(schema));
    }

    private hideDeleteDialog() {
        this.confirmDeleteDialog.hide();
    }

    private notify() {
        this.messageBus.emit(new HistoryChannelUpdated());
    }
}

