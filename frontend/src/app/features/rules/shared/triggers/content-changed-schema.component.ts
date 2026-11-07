/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, input, output } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { CodeEditorComponent, ConfirmClickDirective, ContentChangedSchemaModel, ControlErrorsComponent, SchemaDto, ScriptCompletions, SignalSelectDirective, TranslatePipe } from '@app/shared';
import { CompletionsCache } from './completions-cache';

@Component({
    selector: 'sqx-content-changed-schema',
    styleUrls: ['./content-changed-schema.component.scss'],
    templateUrl: './content-changed-schema.component.html',
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormField,
        FormsModule,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class ContentChangedSchemaComponent {
    private readonly completionsCache = inject(CompletionsCache);

    /** The schemas the user can choose from. */
    public readonly schemas = input.required<ReadonlyArray<SchemaDto> | null | undefined>();

    /** The field with the schema and the condition. */
    public readonly field = input.required<FieldTree<ContentChangedSchemaModel>>();

    /** Emits when the user confirms to remove the schema. */
    public readonly remove = output();

    protected readonly completions = rxResource<ScriptCompletions | null, string | undefined>({
        params: () => this.field().schemaId().value() || undefined,
        stream: ({ params }) => this.completionsCache.getCompletions(params, true),
        defaultValue: null,
    });
}
