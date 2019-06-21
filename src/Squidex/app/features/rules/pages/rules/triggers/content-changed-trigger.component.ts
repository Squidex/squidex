/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

import {
    ImmutableArray,
    SchemaDto,
    Types
} from '@app/shared';

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
    public schemas: ImmutableArray<SchemaDto>;

    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    @Input()
    public triggerFormSubmitted = false;

    public triggerSchemas: ImmutableArray<TriggerSchemaForm>;

    public schemaToAdd: SchemaDto;
    public schemasToAdd: ImmutableArray<SchemaDto>;

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
            for (let triggerSchema of this.trigger.schemas) {
                const schema = this.schemas.find(s => s.id === triggerSchema.schemaId);

                if (schema) {
                    const condition = triggerSchema.condition;

                    schemas.push({ schema, condition });
                }
            }
        }

        this.triggerSchemas = ImmutableArray.of(schemas).sortByStringAsc(s => s.schema.name);

        this.updateSchemaToAdd();
    }

    public removeSchema(schemaForm: TriggerSchemaForm) {
        this.triggerSchemas = this.triggerSchemas.remove(schemaForm);

        this.updateValue();
        this.updateSchemaToAdd();
    }

    public addSchema() {
        this.triggerSchemas = this.triggerSchemas.push({ schema: this.schemaToAdd }).sortByStringAsc(x => x.schema.name);

        this.updateValue();
        this.updateSchemaToAdd();
    }

    public updateCondition(schema: SchemaDto, condition: string) {
        this.triggerSchemas = this.triggerSchemas.map(s => s.schema === schema ? { schema, condition } : s);

        this.updateValue();
    }

    public updateValue() {
        const schemas = this.triggerSchemas.values.map(s => ({ schemaId: s.schema.id, condition: s.condition }));

        this.triggerForm.controls['schemas'].setValue(schemas);
    }

    private updateSchemaToAdd() {
        this.schemasToAdd = this.schemas.filter(schema => !this.triggerSchemas.find(s => s.schema.id === schema.id)).sortByStringAsc(x => x.name);
        this.schemaToAdd = this.schemasToAdd.at(0);
    }

    public trackBySchema(index: number, schema: SchemaDto) {
        return schema.id;
    }
}