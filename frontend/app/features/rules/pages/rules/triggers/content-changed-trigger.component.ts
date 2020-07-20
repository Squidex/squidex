/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { SchemaDto, Types } from '@app/shared';

export interface TriggerSchemaForm {
    schema: SchemaDto;

    condition?: string;
}

@Component({
    selector: 'sqx-content-changed-trigger',
    styleUrls: ['./content-changed-trigger.component.scss'],
    templateUrl: './content-changed-trigger.component.html'
})
export class ContentChangedTriggerComponent implements OnInit {
    @Input()
    public schemas: ReadonlyArray<SchemaDto>;

    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    public triggerSchemas: ReadonlyArray<TriggerSchemaForm>;

    public schemaToAdd: SchemaDto;
    public schemasToAdd: ReadonlyArray<SchemaDto>;

    public get hasSchema() {
        return !!this.schemaToAdd;
    }

    public ngOnInit() {
        this.triggerForm.setControl('schemas',
            new FormControl(this.trigger.schemas || []));

        this.triggerForm.setControl('handleAll',
            new FormControl(Types.isBoolean(this.trigger.handleAll) ? this.trigger.handleAll : false));

        const schemas: TriggerSchemaForm[] = [];

        if (this.trigger.schemas) {
            for (const triggerSchema of this.trigger.schemas) {
                const schema = this.schemas.find(s => s.id === triggerSchema.schemaId);

                if (schema) {
                    const condition = triggerSchema.condition;

                    schemas.push({ schema, condition });
                }
            }
        }

        this.triggerSchemas = schemas.sortedByString(s => s.schema.name);

        this.updateSchemaToAdd();
    }

    public removeSchema(schemaForm: TriggerSchemaForm) {
        this.triggerSchemas = this.triggerSchemas.removed(schemaForm);

        this.updateValue();
        this.updateSchemaToAdd();
    }

    public addSchema() {
        this.triggerSchemas = [{ schema: this.schemaToAdd }, ...this.triggerSchemas].sortedByString(x => x.schema.name);

        this.updateValue();
        this.updateSchemaToAdd();
    }

    public updateCondition(schema: SchemaDto, condition: string) {
        this.triggerSchemas = this.triggerSchemas.map(s => s.schema === schema ? { schema, condition } : s);

        this.updateValue();
    }

    public updateValue() {
        const schemas = this.triggerSchemas.map(s => ({ schemaId: s.schema.id, condition: s.condition }));

        this.triggerForm.controls['schemas'].setValue(schemas);
    }

    private updateSchemaToAdd() {
        this.schemasToAdd = this.schemas.filter(schema => !this.triggerSchemas.find(s => s.schema.id === schema.id)).sortedByString(x => x.name);
        this.schemaToAdd = this.schemasToAdd[0];
    }

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }
}