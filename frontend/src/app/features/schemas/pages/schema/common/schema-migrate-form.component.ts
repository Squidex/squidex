/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, FormRowComponent, MarkdownDirective, MigrateContentsDto, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-migrate-form',
    templateUrl: './schema-migrate-form.component.html',
    imports: [
        ConfirmClickDirective,
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class SchemaMigrateFormComponent {
    @Input({ required: true })
    public schema!: SchemaDto;

    public migrateDraft = true;

    public migratePublished = true;

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public migrateContents() {
        if (!this.schema.canContentsMigrate || (!this.migrateDraft && !this.migratePublished)) {
            return;
        }

        const request = new MigrateContentsDto({
            migrateDraft: this.migrateDraft,
            migratePublished: this.migratePublished,
        });

        this.schemasState.migrateContents(this.schema, request).subscribe();
    }
}
