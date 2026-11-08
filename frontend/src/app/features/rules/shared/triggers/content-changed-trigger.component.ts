/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { CodeComponent, ContentChangedTriggerForm, FormHintComponent, SchemaDto, TranslatePipe } from '@app/shared';
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
        FormField,
        FormHintComponent,
        TranslatePipe,
    ],
})
export class ContentChangedTriggerComponent {
    /** The schemas the user can choose from. */
    public readonly schemas = input<ReadonlyArray<SchemaDto> | null>();

    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<ContentChangedTriggerForm>();

    protected addSchema() {
        this.triggerForm().addSchema();
    }

    protected addReferencedSchema() {
        this.triggerForm().addReferencedSchema();
    }

    protected remove(index: number) {
        this.triggerForm().removeSchema(index);
    }

    protected removeReferencedSchema(index: number) {
        this.triggerForm().removeReferencedSchema(index);
    }
}
