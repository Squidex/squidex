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
    WebhookDto
} from 'shared';

interface WebhookSchemaForm {
    schema: SchemaDto;
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

    public schemasToAdd: ImmutableArray<SchemaDto>;

    public webhookFormSubmitted = false;
    public webhookForm =
        this.formBuilder.group({
            url: ['',
                [
                    Validators.required
                ]]
        });

    public addSchemaForm =
        this.formBuilder.group({
            schema: [null]
        });

    public get hasUrl() {
        return this.webhookForm.controls['url'].value && this.webhookForm.controls['url'].value.length > 0;
    }

    public get hasSchema() {
        return this.addSchemaForm.controls['schema'].value;
    }

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.webhookForm.controls['url'].setValue(this.webhook.url);

        this.schemasToAdd =
            ImmutableArray.of(
                this.allSchemas.filter(schema =>
                    !this.webhook.schemas.find(w => w.schemaId === schema.id)))
                .sortByStringAsc(x => x.name);

        this.schemas =
            ImmutableArray.of(
                this.webhook.schemas.map(webhookSchema => {
                    const schema = this.allSchemas.find(s => s.id === webhookSchema.schemaId);

                    if (schema) {
                        return {
                            schema: schema,
                            sendCreate: webhookSchema.sendCreate,
                            sendUpdate: webhookSchema.sendUpdate,
                            sendDelete: webhookSchema.sendDelete,
                            sendPublish: webhookSchema.sendPublish,
                            sendUnpublish: webhookSchema.sendUnpublish
                        };
                    } else {
                        return null;
                    }
                }).filter(w => !!w)).sortByStringAsc(x => x.schema.name);

        this.addSchemaForm.controls['schema'].setValue(this.schemasToAdd.find(x => true));
    }

    public removeSchema(webhookSchema: WebhookSchemaForm) {
        this.schemasToAdd = this.schemasToAdd.push(webhookSchema.schema).sortByStringAsc(x => x.name);
        this.schemas = this.schemas.remove(webhookSchema);

        this.addSchemaForm.controls['schema'].setValue(this.schemasToAdd.find(x => true));
    }

    public addSchema() {
        const schema: SchemaDto = this.addSchemaForm.controls['schema'].value;

        this.schemasToAdd = this.schemasToAdd.remove(schema).sortByStringAsc(x => x.name);
        this.schemas = this.schemas.push({
            schema: schema,
            sendCreate: false,
            sendUpdate: false,
            sendDelete: false,
            sendPublish: false,
            sendUnpublish: false
        }).sortByStringAsc(x => x.schema.name);

        this.addSchemaForm.controls['schema'].setValue(this.schemasToAdd.find(x => true));
    }
}
