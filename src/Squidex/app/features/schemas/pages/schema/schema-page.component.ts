/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

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
    SchemaDto,
    SchemaPropertiesDto,
    SchemasService,
    UpdateFieldDto,
    ValidatorsEx,
    Version
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
    public fieldTypes: string[] = [
        'Assets',
        'Boolean',
        'DateTime',
        'Geolocation',
        'Json',
        'Number',
        'References',
        'String'
    ];

    public schemaExport: any;
    public schemaName: string;
    public schemaFields = ImmutableArray.empty<FieldDto>();
    public schemaVersion = new Version('');
    public schemaProperties: SchemaPropertiesDto;
    public schemaInformation: any;
    public schemas: SchemaDto[];

    public confirmDeleteDialog = new ModalView();

    public exportSchemaDialog = new ModalView();

    public editOptionsDropdown = new ModalView();
    public editSchemaDialog = new ModalView();

    public isPublished: boolean;

    public addFieldFormSubmitted = false;
    public addFieldForm: FormGroup =
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

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly schemasService: SchemasService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        this.route.data.map(p => p['schema'])
            .subscribe((schema: SchemaDetailsDto) => {
                this.schemaName = schema.name;
                this.schemaFields = ImmutableArray.of(schema.fields);
                this.schemaVersion = schema.version;
                this.schemaProperties = schema.properties;
                this.schemaInformation = { properties: schema.properties, name: schema.name };

                this.isPublished = schema.isPublished;

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
            .switchMap(app => this.schemasService.publishSchema(app, this.schemaName, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.isPublished = true;
                this.notify();
            }, error => {
                this.notifyError(error);
            });
    }

    public unpublish() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.unpublishSchema(app, this.schemaName, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.isPublished = false;
                this.notify();
            }, error => {
                this.notifyError(error);
            });
    }

    public enableField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.enableField(app, this.schemaName, field.fieldId, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, field.isHidden, false, field.partitioning, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public disableField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.disableField(app, this.schemaName, field.fieldId, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, field.isHidden, true, field.partitioning, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public showField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.showField(app, this.schemaName, field.fieldId, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, false, field.isDisabled, field.partitioning, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public hideField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.hideField(app, this.schemaName, field.fieldId, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, true, field.isDisabled, field.partitioning, field.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteField(field: FieldDto) {
        this.appNameOnce()
            .switchMap(app => this.schemasService.deleteField(app, this.schemaName, field.fieldId, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateFields(this.schemaFields.remove(field));
            }, error => {
                this.notifyError(error);
            });
    }

    public sortFields(fields: FieldDto[]) {
        this.updateFields(ImmutableArray.of(fields));

        this.appNameOnce()
            .switchMap(app => this.schemasService.putFieldOrdering(app, this.schemaName, fields.map(t => t.fieldId), this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateFields(ImmutableArray.of(fields));
            }, error => {
                this.notifyError(error);
            });
    }

    public saveField(field: FieldDto, newField: FieldDto) {
        const requestDto = new UpdateFieldDto(newField.properties);

        this.appNameOnce()
            .switchMap(app => this.schemasService.putField(app, this.schemaName, field.fieldId, requestDto, this.schemaVersion)).retry(2)
            .subscribe(() => {
                this.updateField(field, new FieldDto(field.fieldId, field.name, newField.isHidden, field.isDisabled, field.partitioning, newField.properties));
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteSchema() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.deleteSchema(app, this.schemaName, this.schemaVersion)).retry(2)
            .finally(() => {
                this.confirmDeleteDialog.hide();
            })
            .subscribe(() => {
                this.messageBus.publish(new SchemaDeleted(this.schemaName));

                this.router.navigate(['../'], { relativeTo: this.route });
            }, error => {
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

            const reset = () => {
                this.addFieldForm.reset({ type: 'String' });
                this.addFieldForm.enable();
                this.addFieldFormSubmitted = false;
            };

            this.appNameOnce()
                .switchMap(app => this.schemasService.postField(app, this.schemaName, requestDto, this.schemaVersion))
                .subscribe(dto => {
                    const newField =
                        new FieldDto(parseInt(dto.id, 10),
                            requestDto.name,
                            false,
                            false,
                            requestDto.partitioning,
                            requestDto.properties);

                    this.updateFields(this.schemaFields.push(newField));
                    reset();
                }, error => {
                    this.notifyError(error);
                    reset();
                });
        }
    }

    public resetFieldForm() {
        this.addFieldForm.reset({ type: 'String' });
        this.addFieldFormSubmitted = false;
    }

    public onSchemaSaved(properties: SchemaPropertiesDto) {
        this.updateProperties(properties);

        this.editSchemaDialog.hide();
    }

    private updateProperties(properties: SchemaPropertiesDto) {
        this.schemaProperties = properties;
        this.schemaInformation = { properties: properties, name: this.schemaName };

        this.notify();
        this.export();
    }

    private updateField(field: FieldDto, newField: FieldDto) {
        this.schemaFields = this.schemaFields.replace(field, newField);

        this.notify();
        this.export();
    }

    private updateFields(fields: ImmutableArray<FieldDto>) {
        this.schemaFields = fields;

        this.notify();
        this.export();
    }

    private export() {
        const result: any = {
            fields: this.schemaFields.values.map(field => {
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

        if (this.schemaProperties.label) {
            result.properties.label = this.schemaProperties.label;
        }

        if (this.schemaProperties.hints) {
            result.properties.hints = this.schemaProperties.hints;
        }

        this.schemaExport = result;
    }

    private notify() {
        this.messageBus.publish(new HistoryChannelUpdated());
        this.messageBus.publish(new SchemaUpdated(this.schemaName, this.schemaProperties, this.isPublished, this.schemaVersion.value));
    }
}

