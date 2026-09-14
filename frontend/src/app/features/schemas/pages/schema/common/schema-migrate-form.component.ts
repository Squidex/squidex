/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { ConfirmClickDirective, MarkdownDirective, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-migrate-form',
    templateUrl: './schema-migrate-form.component.html',
    imports: [
        ConfirmClickDirective,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class SchemaMigrateFormComponent {
    @Input({ required: true })
    public schema!: SchemaDto;

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public migrateContents() {
        if (!this.schema.canContentsMigrate) {
            return;
        }

        this.schemasState.migrateContents(this.schema).subscribe();
    }
}
