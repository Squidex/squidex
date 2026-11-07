/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input, output, inject } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';
import { CodeEditorComponent, ConfirmClickDirective, ControlErrorsComponent, SchemaDto, TranslatePipe, value$ } from '@app/shared';
import { CompletionsCache } from './completions-cache';

@Component({
    selector: 'sqx-content-changed-schema',
    styleUrls: ['./content-changed-schema.component.scss'],
    templateUrl: './content-changed-schema.component.html',
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class ContentChangedSchemaComponent {
    /** The schemas the user can choose from. */
    public readonly schemas = input.required<ReadonlyArray<SchemaDto> | null | undefined>();

    /** The form group with the schema and the condition. */
    public readonly form = input.required<FormGroup>();

    /** Emits when the user confirms to remove the schema. */
    public readonly remove = output();

    protected readonly completions;

    constructor() {
        const completionsCache = inject(CompletionsCache);

        this.completions = toSignal(
            toObservable(this.form).pipe(
                switchMap(form => value$(form.controls['schemaId'])),
                switchMap(schemaId => completionsCache.getCompletions(schemaId, true))),
            { initialValue: null });
    }
}
