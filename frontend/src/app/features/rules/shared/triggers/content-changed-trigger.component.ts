/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { SchemaDto, TriggerForm } from '@app/shared';

export interface TriggerSchemaForm {
    schema: SchemaDto;

    condition?: string;
}

@Component({
    selector: 'sqx-content-changed-trigger[trigger][triggerForm]',
    styleUrls: ['./content-changed-trigger.component.scss'],
    templateUrl: './content-changed-trigger.component.html',
})
export class ContentChangedTriggerComponent implements OnChanges {
    @Input()
    public schemas?: ReadonlyArray<SchemaDto> | null;

    @Input()
    public trigger!: any;

    @Input()
    public triggerForm!: TriggerForm;

    public triggerSchemas: TriggerSchemaForm[] = [];

    public schemaToAdd!: SchemaDto;
    public schemasToAdd!: ReadonlyArray<SchemaDto>;

    public get hasSchema() {
        return !!this.schemaToAdd;
    }

    public ngOnChanges() {
        const schemas: TriggerSchemaForm[] = [];

        if (this.trigger.schemas && this.schemas) {
            for (const triggerSchema of this.trigger.schemas) {
                const schema = this.schemas.find(s => s.id === triggerSchema.schemaId);

                if (schema) {
                    const condition = triggerSchema.condition;

                    schemas.push({ schema, condition });
                }
            }
        }

        this.triggerSchemas = schemas;
        this.triggerSchemas.sortByString(x => x.schema.name);

        this.updateSchemaToAdd();
    }

    public removeSchema(schemaForm: TriggerSchemaForm) {
        this.triggerSchemas.remove(schemaForm);

        this.updateValue();
        this.updateSchemaToAdd();
    }

    public addSchema() {
        this.triggerSchemas.push({ schema: this.schemaToAdd });
        this.triggerSchemas.sortByString(x => x.schema.name);

        this.updateValue();
        this.updateSchemaToAdd();
    }

    public updateCondition(schema: SchemaDto, condition: string) {
        this.triggerSchemas.replaceBy('schema', { schema, condition });

        this.updateValue();
    }

    public updateValue() {
        const schemas = this.triggerSchemas.map(({ schema, condition }) => ({ schemaId: schema.id, condition }));

        this.triggerForm.form.patchValue({ schemas });
    }

    private updateSchemaToAdd() {
        this.schemasToAdd = this.schemas?.filter(schema => !this.triggerSchemas.find(s => s.schema.id === schema.id)).sortByString(x => x.name) || [];
        this.schemaToAdd = this.schemasToAdd[0];
    }

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }
}
