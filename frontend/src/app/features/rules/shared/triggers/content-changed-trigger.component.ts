/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeComponent, FormHintComponent, SchemaDto, TemplatedFormArray, TranslatePipe, TriggerForm } from '@app/shared';
import { CompletionsCache } from './completions-cache';
import { ContentChangedSchemaComponent } from './content-changed-schema.component';

@Component({
    standalone: true,
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
    @Input()
    public schemas?: ReadonlyArray<SchemaDto> | null;

    @Input({ required: true })
    public trigger!: any;

    @Input({ required: true })
    public triggerForm!: TriggerForm;

    public get schemasForm() {
        return this.triggerForm.form.get('schemas') as TemplatedFormArray;
    }

    public get referencedSchemasForm() {
        return this.triggerForm.form.get('referencedSchemas') as TemplatedFormArray;
    }

    public addSchema() {
        this.schemasForm.add();
    }

    public addReferencedSchema() {
        this.referencedSchemasForm.add();
    }

    public removeSchema(index: number) {
        this.schemasForm.removeAt(index);
    }

    public removeReferencedSchema(index: number) {
        this.referencedSchemasForm.removeAt(index);
    }
}
