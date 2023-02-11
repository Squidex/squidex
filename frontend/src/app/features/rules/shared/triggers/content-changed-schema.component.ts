/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { EMPTY, Observable, switchMap } from 'rxjs';
import { SchemaCompletions, SchemaDto, TypedSimpleChanges, value$ } from '@app/shared';
import { CompletionsCache } from './completions-cache';

@Component({
    selector: 'sqx-content-changed-schema[form][schemas]',
    styleUrls: ['./content-changed-schema.component.scss'],
    templateUrl: './content-changed-schema.component.html',
})
export class ContentChangedSchemaComponent {
    @Input()
    public schemas?: ReadonlyArray<SchemaDto> | null;

    @Input()
    public form!: FormGroup;

    @Output()
    public remove = new EventEmitter<any>();

    public completions: Observable<SchemaCompletions> = EMPTY;

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

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }
}
