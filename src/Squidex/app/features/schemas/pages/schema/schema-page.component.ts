/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import {
    AddFieldDto,
    AppComponentBase,
    AppsStoreService,
    createProperties,
    fadeAnimation,
    FieldDto,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    ModalView,
    NotificationService,
    SchemaDetailsDto,
    SchemasService,
    UpdateFieldDto,
    UsersProviderService,
    ValidatorsEx,
    Version
} from 'shared';

import { SchemaPropertiesDto } from './schema-properties';
import { SchemaUpdated } from './../messages';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaPageComponent extends AppComponentBase implements OnInit {
    public fieldTypes: string[] = [
        'Boolean',
        'DateTime',
        'Json',
        'Number',
        'String'
    ];

    public schemaName: string;
    public schemaFields = ImmutableArray.empty<FieldDto>();
    public schemaProperties: SchemaPropertiesDto;

    public version = new Version('');

    public editSchemaDialog = new ModalView();

    public isPublished: boolean;

    public addFieldFormSubmitted = false;
    public addFieldForm: FormGroup =
        this.formBuilder.group({
            type: [this.fieldTypes[0],
                [
                    Validators.required
                ]],
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'Name must be a valid javascript name in camel case.')
                ]]
        });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly schemasService: SchemasService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute
    ) {
        super(notifications, users, apps);
    }

    public ngOnInit() {
        this.route.data.map(p => p['schema'])
            .subscribe((schema: SchemaDetailsDto) => {
                this.schemaName = schema.name;
                this.schemaFields = ImmutableArray.of(schema.fields);
                this.schemaProperties = new SchemaPropertiesDto(schema.name, schema.label, schema.hints);

                this.version = schema.version;

                this.isPublished = schema.isPublished;
            });
    }

    public publish() {
        this.appName()
            .switchMap(app => this.schemasService.publishSchema(app, this.schemaName, this.version)).retry(2)
            .subscribe(() => {
                this.isPublished = true;
                this.notify();
            }, error => {
                this.notifyError(error);
            });
    }

    public unpublish() {
        this.appName()
            .switchMap(app => this.schemasService.unpublishSchema(app, this.schemaName, this.version)).retry(2)
            .subscribe(() => {
                this.isPublished = false;
                this.notify();
            }, error => {
                this.notifyError(error);
            });
    }

    public enableField(field: FieldDto) {
        this.appName()
            .switchMap(app => this.schemasService.enableField(app, this.schemaName, field.fieldId, this.version)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, field.isHidden, false, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public disableField(field: FieldDto) {
        this.appName()
            .switchMap(app => this.schemasService.disableField(app, this.schemaName, field.fieldId, this.version)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, field.isHidden, true, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public showField(field: FieldDto) {
        this.appName()
            .switchMap(app => this.schemasService.showField(app, this.schemaName, field.fieldId, this.version)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, false, field.isDisabled, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public hideField(field: FieldDto) {
        this.appName()
            .switchMap(app => this.schemasService.hideField(app, this.schemaName, field.fieldId, this.version)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, true, field.isDisabled, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteField(field: FieldDto) {
        this.appName()
            .switchMap(app => this.schemasService.deleteField(app, this.schemaName, field.fieldId, this.version)).retry(2)
            .subscribe(() => {
                this.updateFields(this.schemaFields.remove(field));
            }, error => {
                this.notifyError(error);
            });
    }

    public saveField(field: FieldDto, newField: FieldDto) {
        const request = new UpdateFieldDto(newField.properties);

        this.appName()
            .switchMap(app => this.schemasService.putField(app, this.schemaName, field.fieldId, request, this.version)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, newField.isHidden, field.isDisabled, newField.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public addField() {
        this.addFieldFormSubmitted = true;

        if (this.addFieldForm.valid) {
            this.addFieldForm.disable();

            const properties = createProperties(this.addFieldForm.get('type').value);

            const requestDto = new AddFieldDto(this.addFieldForm.get('name').value, properties);

            const reset = () => {
                this.addFieldForm.get('name').reset();
                this.addFieldForm.enable();
                this.addFieldFormSubmitted = false;
            };

            this.appName()
                .switchMap(app => this.schemasService.postField(app, this.schemaName, requestDto, this.version))
                .subscribe(dto => {
                    const newField =
                        new FieldDto(parseInt(dto.id, 10),
                            this.addFieldForm.get('name').value,
                            false,
                            false,
                            properties);

                    this.updateFields(this.schemaFields.push(newField));
                    reset();
                }, error => {
                    this.notifyError(error);
                    reset();
                });
        }
    }

    public resetFieldForm() {
        this.addFieldFormSubmitted = false;
        this.addFieldForm.reset();
    }

    public onSchemaSaved(properties: SchemaPropertiesDto) {
        this.updateProperties(properties);

        this.editSchemaDialog.hide();
    }

    private updateProperties(properties: SchemaPropertiesDto) {
        this.schemaProperties = properties;

        this.notify();
    }

    private updateField(field: FieldDto, newField: FieldDto) {
        this.schemaFields = this.schemaFields.replace(field, newField);

        this.notify();
    }

    private updateFields(fields: ImmutableArray<FieldDto>) {
        this.schemaFields = fields;

        this.notify();
    }

    private notify() {
        this.messageBus.publish(new HistoryChannelUpdated());
        this.messageBus.publish(new SchemaUpdated(this.schemaName, this.schemaProperties.label, this.isPublished, this.version.value));
    }
}

