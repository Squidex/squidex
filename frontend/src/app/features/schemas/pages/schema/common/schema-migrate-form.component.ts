/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { form, FormField } from '@angular/forms/signals';
import { ConfirmClickDirective, FormRowComponent, MarkdownDirective, MigrateContentsDto, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-migrate-form',
    templateUrl: './schema-migrate-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FormField,
        FormRowComponent,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class SchemaMigrateFormComponent {
    private readonly schemasState = inject(SchemasState);

    /** The schema to migrate the contents for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly migrateDraft = form(signal(true));

    protected readonly migratePublished = form(signal(true));

    protected migrateContents() {
        const schema = this.schema();
        if (!schema.canContentsMigrate) {
            return;
        }

        const migrateDraft = this.migrateDraft().value();
        const migratePublished = this.migratePublished().value();
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
