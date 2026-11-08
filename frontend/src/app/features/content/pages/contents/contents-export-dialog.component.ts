/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { form, FormField } from '@angular/forms/signals';
import { AppLanguageDto, ExportContentsDto, ExportFormat, FormRowComponent, MarkdownDirective, ModalDialogComponent, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

type FieldPaths = { label: string; paths: ReadonlyArray<string> };

@Component({
    selector: 'sqx-contents-export-dialog',
    styleUrls: ['./contents-export-dialog.component.scss'],
    templateUrl: './contents-export-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
        MarkdownDirective,
        ModalDialogComponent,
        TranslatePipe,
    ],
})
export class ContentsExportDialogComponent {
    private readonly schemasState = inject(SchemasState);

    /** Emits when the dialog is closed or the export has been started. */
    public readonly dialogClose = output();

    /** The schema to export the contents for. */
    public readonly schema = input.required<SchemaDto>();

    /** All languages of the app, to build the paths of localized fields. */
    public readonly languages = input<ReadonlyArray<AppLanguageDto>>([]);

    protected readonly fieldPaths = computed<ReadonlyArray<FieldPaths>>(() => {
        const languages = this.languages();

        return this.schema().fields.filter(x => x.properties.isContentField).map(field => {
            const paths =
                field.isLocalizable ?
                languages.map(x => `data.${field.name}.${x.iso2Code}`) :
                [`data.${field.name}`];

            return { label: field.displayName, paths };
        });
    });

    protected readonly metaPaths = ['id', 'created', 'createdBy', 'lastModified', 'lastModifiedBy', 'status', 'newStatus', 'version'];

    protected readonly exportForm = form(signal({ format: 'Csv' as ExportFormat, fields: '', unpublished: false }));

    protected addPath(path: string) {
        const fields = this.exportForm.fields().value;
        const fieldsCurrent = fields().trim();

        fields.set(fieldsCurrent ? `${fieldsCurrent}, ${path}` : path);
    }

    protected export() {
        const schema = this.schema();
        if (!schema.canContentsExport) {
            return;
        }

        const { format, fields, unpublished } = this.exportForm().value();

        const request = new ExportContentsDto({
            format,
            fields: fields.trim() || undefined,
            unpublished,
        });

        this.schemasState.exportContents(schema, request)
            .subscribe(() => {
                this.dialogClose.emit();
            });
    }
}
