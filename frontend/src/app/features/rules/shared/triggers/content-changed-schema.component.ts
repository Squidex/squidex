/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EMPTY, Observable, switchMap } from 'rxjs';
import { CodeEditorComponent, ConfirmClickDirective, ControlErrorsComponent, SchemaDto, ScriptCompletions, TranslatePipe, TypedSimpleChanges, value$ } from '@app/shared';
import { CompletionsCache } from './completions-cache';

@Component({
    standalone: true,
    selector: 'sqx-content-changed-schema',
    styleUrls: ['./content-changed-schema.component.scss'],
    templateUrl: './content-changed-schema.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class ContentChangedSchemaComponent {
    @Input({ required: true })
    public schemas?: ReadonlyArray<SchemaDto> | null;

    @Input({ required: true })
    public form!: FormGroup;

    @Output()
    public remove = new EventEmitter<any>();

    public completions: Observable<ScriptCompletions> = EMPTY;

    constructor(
        private readonly completionsCache: CompletionsCache,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.form) {
            this.completions =
                value$(this.form.controls['schemaId']).pipe(
                    switchMap(x => this.completionsCache.getCompletions(x, true)));
        }
    }
}
