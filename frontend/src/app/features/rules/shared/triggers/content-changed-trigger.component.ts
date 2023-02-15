/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { SchemaDto, TemplatedFormArray, TriggerForm } from '@app/shared';
import { CompletionsCache } from './completions-cache';

@Component({
    selector: 'sqx-content-changed-trigger[trigger][triggerForm]',
    styleUrls: ['./content-changed-trigger.component.scss'],
    templateUrl: './content-changed-trigger.component.html',
    providers: [
        CompletionsCache,
    ],
})
export class ContentChangedTriggerComponent {
    @Input()
    public schemas?: ReadonlyArray<SchemaDto> | null;

    @Input()
    public trigger!: any;

    @Input()
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
