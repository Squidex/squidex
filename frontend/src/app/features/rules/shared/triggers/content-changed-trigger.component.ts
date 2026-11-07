/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeComponent, FormHintComponent, SchemaDto, TemplatedFormArray, TranslatePipe, TriggerForm } from '@app/shared';
import { CompletionsCache } from './completions-cache';
import { ContentChangedSchemaComponent } from './content-changed-schema.component';

@Component({
    selector: 'sqx-content-changed-trigger',
    styleUrls: ['./content-changed-trigger.component.scss'],
    templateUrl: './content-changed-trigger.component.html',
    providers: [
        CompletionsCache,
    ],
    imports: [
        CodeComponent,
        ContentChangedSchemaComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class ContentChangedTriggerComponent {
    /** The schemas the user can choose from. */
    public readonly schemas = input<ReadonlyArray<SchemaDto> | null>();

    /** The current trigger settings. */
    public readonly triggerDto = input.required<any>();

    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<TriggerForm>();

    public readonly schemasForm = computed(() => {
        return this.triggerForm().form.get('schemas') as TemplatedFormArray;
    });

    public readonly referencedSchemasForm = computed(() => {
        return this.triggerForm().form.get('referencedSchemas') as TemplatedFormArray;
    });

    public addSchema() {
        this.schemasForm().add();
    }

    public addReferencedSchema() {
        this.referencedSchemasForm().add();
    }

    public remove(index: number) {
        this.schemasForm().removeAt(index);
    }

    public removeReferencedSchema(index: number) {
        this.referencedSchemasForm().removeAt(index);
    }
}
