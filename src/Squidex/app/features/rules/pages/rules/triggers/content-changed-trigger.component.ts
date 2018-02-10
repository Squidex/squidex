/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import {
    ImmutableArray,
    SchemaDto,
    Types
} from 'shared';

export interface TriggerSchemaForm {
    schema: SchemaDto;
    sendAll: boolean;
    sendCreate: boolean;
    sendUpdate: boolean;
    sendDelete: boolean;
    sendPublish: boolean;
}

@Component({
    selector: 'sqx-content-changed-trigger',
    styleUrls: ['./content-changed-trigger.component.scss'],
    templateUrl: './content-changed-trigger.component.html'
})
export class ContentChangedTriggerComponent implements OnInit {
    @Input()
    public schemas: SchemaDto[];

    @Input()
    public trigger: any;

    @Output()
    public triggerChanged = new EventEmitter<object>();

    public handleAll = false;

    public triggerSchemas: ImmutableArray<TriggerSchemaForm>;

    public schemaToAdd: SchemaDto;
    public schemasToAdd: ImmutableArray<SchemaDto>;

    public get hasSchema() {
        return !!this.schemaToAdd;
    }

    public ngOnInit() {
        const triggerSchemas: any[] = (this.trigger.schemas = this.trigger.schemas || []);

        this.handleAll = Types.isBoolean(this.trigger.handleAll) ? this.trigger.handleAll : false;

        this.triggerSchemas =
            ImmutableArray.of(
                triggerSchemas.map(triggerSchema => {
                    const schema = this.schemas.find(s => s.id === triggerSchema.schemaId);

                    if (schema) {
                        return this.updateSendAll({
                            schema: schema,
                            sendAll: false,
                            sendCreate: triggerSchema.sendCreate,
                            sendUpdate: triggerSchema.sendUpdate,
                            sendDelete: triggerSchema.sendDelete,
                            sendPublish: triggerSchema.sendPublish
                        });
                    } else {
                        return null;
                    }
                }).filter(s => s !== null).map(s => s!)).sortByStringAsc(s => s.schema.name);

        this.schemasToAdd =
            ImmutableArray.of(
                this.schemas.filter(schema =>
                    !triggerSchemas.find(s => s.schemaId === schema.id)))
                .sortByStringAsc(x => x.name);

        this.schemaToAdd = this.schemasToAdd.values[0];
    }

    public save() {
        const schemas =
            this.triggerSchemas.values.map(s => {
                return {
                    schemaId: s.schema.id,
                    sendCreate: s.sendCreate,
                    sendUpdate: s.sendUpdate,
                    sendDelete: s.sendDelete,
                    sendPublish: s.sendPublish
                };
            });

        this.triggerChanged.emit({ schemas, handleAll: this.handleAll });
    }

    public removeSchema(schemaForm: TriggerSchemaForm) {
        this.triggerSchemas = this.triggerSchemas.remove(schemaForm);

        this.schemasToAdd = this.schemasToAdd.push(schemaForm.schema).sortByStringAsc(x => x.name);
        this.schemaToAdd = this.schemasToAdd.values[0];
    }

    public addSchema() {
        this.triggerSchemas =
            this.triggerSchemas.push(
                this.updateSendAll({
                    schema: this.schemaToAdd,
                    sendAll: false,
                    sendCreate: false,
                    sendUpdate: false,
                    sendDelete: false,
                    sendPublish: false
                })).sortByStringAsc(x => x.schema.name);

        this.schemasToAdd = this.schemasToAdd.remove(this.schemaToAdd).sortByStringAsc(x => x.name);
        this.schemaToAdd = this.schemasToAdd.values[0];
    }

    public toggle(schemaForm: TriggerSchemaForm, property: string) {
        const newSchema = this.updateSendAll(Object.assign({}, schemaForm, { [property]: !schemaForm[property] }));

        this.triggerSchemas = this.triggerSchemas.replace(schemaForm, newSchema);
    }

    public toggleAll(schemaForm: TriggerSchemaForm) {
        const newSchema = this.updateAll(<any>{ schema: schemaForm.schema }, !schemaForm.sendAll);

        this.triggerSchemas = this.triggerSchemas.replace(schemaForm, newSchema);
    }

    private updateAll(schemaForm: TriggerSchemaForm, value: boolean): TriggerSchemaForm {
        schemaForm.sendAll = value;
        schemaForm.sendCreate = value;
        schemaForm.sendUpdate = value;
        schemaForm.sendDelete = value;
        schemaForm.sendPublish = value;
        return schemaForm;
    }

    private updateSendAll(schemaForm: TriggerSchemaForm): TriggerSchemaForm {
        schemaForm.sendAll =
            schemaForm.sendCreate &&
            schemaForm.sendUpdate &&
            schemaForm.sendDelete &&
            schemaForm.sendPublish;

        return schemaForm;
    }
}