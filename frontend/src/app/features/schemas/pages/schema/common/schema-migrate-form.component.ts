/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, FormRowComponent, MarkdownDirective, MigrateContentsDto, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-migrate-form',
    templateUrl: './schema-migrate-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class SchemaMigrateFormComponent {
    private readonly schemasState = inject(SchemasState);

    /** The schema to migrate the contents for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly migrateDraft = signal(true);

    protected readonly migratePublished = signal(true);

    protected migrateContents() {
        const schema = this.schema();
        if (!schema.canContentsMigrate) {
            return;
        }

        const migrateDraft = this.migrateDraft();
        const migratePublished = this.migratePublished();
        if (!migrateDraft && !migratePublished) {
            return;
        }

        const request = new MigrateContentsDto({
            migrateDraft,
            migratePublished,
        });

        this.schemasState.migrateContents(schema, request).subscribe();
    }
}
