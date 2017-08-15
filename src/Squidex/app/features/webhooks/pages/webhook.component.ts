/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ImmutableArray,
    SchemaDto,
    UpdateWebhookDto,
    WebhookDto,
    WebhookSchemaDto
} from 'shared';

export interface WebhookSchemaForm {
    schema: SchemaDto;
    sendAll: boolean;
    sendCreate: boolean;
    sendUpdate: boolean;
    sendDelete: boolean;
    sendPublish: boolean;
    sendUnpublish: boolean;
}

@Component({
    selector: 'sqx-webhook',
    styleUrls: ['./webhook.component.scss'],
    templateUrl: './webhook.component.html'
})
export class WebhookComponent implements OnInit {
    @Output()
    public deleting = new EventEmitter();

    @Output()
    public updating = new EventEmitter<UpdateWebhookDto>();

    @Input()
    public allSchemas: SchemaDto[];

    @Input()
    public webhook: WebhookDto;

    public schemas: ImmutableArray<WebhookSchemaForm>;

    public schemaToAdd: SchemaDto;
    public schemasToAdd: ImmutableArray<SchemaDto>;

    public webhookForm =
        this.formBuilder.group({
            url: ['',
                [
                    Validators.required
                ]]
        });

    public get hasUrl() {
        return this.webhookForm.controls['url'].value && this.webhookForm.controls['url'].value.length > 0;
    }

    public get hasSchema() {
        return !!this.schemaToAdd;
    }

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.webhookForm.controls['url'].setValue(this.webhook.url);

        this.schemas =
            ImmutableArray.of(
                this.webhook.schemas.map(webhookSchema => {
                    const schema = this.allSchemas.find(s => s.id === webhookSchema.schemaId);

                    if (schema) {
                        return this.updateSendAll({
                            schema: schema,
                            sendAll: false,
                            sendCreate: webhookSchema.sendCreate,
                            sendUpdate: webhookSchema.sendUpdate,
                            sendDelete: webhookSchema.sendDelete,
                            sendPublish: webhookSchema.sendPublish,
                            sendUnpublish: webhookSchema.sendUnpublish
                        });
                    } else {
                        return null;
                    }
                }).filter(w => !!w)).sortByStringAsc(x => x.schema.name);

        this.schemasToAdd =
            ImmutableArray.of(
                this.allSchemas.filter(schema =>
                    !this.webhook.schemas.find(w => w.schemaId === schema.id)))
                .sortByStringAsc(x => x.name);
        this.schemaToAdd = this.schemasToAdd.values[0];
    }

    public removeSchema(schemaForm: WebhookSchemaForm) {
        this.schemas = this.schemas.remove(schemaForm);

        this.schemasToAdd = this.schemasToAdd.push(schemaForm.schema).sortByStringAsc(x => x.name);
        this.schemaToAdd = this.schemasToAdd.values[0];
    }

    public addSchema() {
        this.schemas =
            this.schemas.push(
                this.updateSendAll({
                    schema: this.schemaToAdd,
                    sendAll: false,
                    sendCreate: false,
                    sendUpdate: false,
                    sendDelete: false,
                    sendPublish: false,
                    sendUnpublish: false
                })).sortByStringAsc(x => x.schema.name);

        this.schemasToAdd = this.schemasToAdd.remove(this.schemaToAdd).sortByStringAsc(x => x.name);
        this.schemaToAdd = this.schemasToAdd.values[0];
    }

    public save() {
        const requestDto =
            new UpdateWebhookDto(
                this.webhookForm.controls['url'].value,
                this.schemas.values.map(schema =>
                    new WebhookSchemaDto(
                        schema.schema.id,
                        schema.sendCreate,
                        schema.sendUpdate,
                        schema.sendDelete,
                        schema.sendPublish,
                        schema.sendUnpublish)));

        this.emitUpdating(requestDto);
    }

    public toggle(schemaForm: WebhookSchemaForm, property: string) {
        const newSchema = this.updateSendAll(Object.assign({}, schemaForm, { [property]: !schemaForm[property] }));

        this.schemas = this.schemas.replace(schemaForm, newSchema);
    }

    public toggleAll(schemaForm: WebhookSchemaForm) {
        const newSchema = this.updateAll(<any>{ schema: schemaForm.schema }, !schemaForm.sendAll);

        this.schemas = this.schemas.replace(schemaForm, newSchema);
    }

    private emitUpdating(dto: UpdateWebhookDto) {
        this.updating.emit(dto);
    }

    private updateAll(schemaForm: WebhookSchemaForm, value: boolean): WebhookSchemaForm {
        schemaForm.sendAll = value;
        schemaForm.sendCreate = value;
        schemaForm.sendUpdate = value;
        schemaForm.sendDelete = value;
        schemaForm.sendPublish = value;
        schemaForm.sendUnpublish = value;

        return schemaForm;
    }

    private updateSendAll(schemaForm: WebhookSchemaForm): WebhookSchemaForm {
        schemaForm.sendAll =
            schemaForm.sendCreate &&
            schemaForm.sendUpdate &&
            schemaForm.sendDelete &&
            schemaForm.sendPublish &&
            schemaForm.sendUnpublish;

        return schemaForm;
    }
}
