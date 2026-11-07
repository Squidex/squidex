/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppLanguageDto, ExportContentsDto, ExportFormat, FormRowComponent, MarkdownDirective, ModalDialogComponent, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

type FieldPaths = { label: string; paths: ReadonlyArray<string> };

@Component({
    selector: 'sqx-contents-export-dialog',
    styleUrls: ['./contents-export-dialog.component.scss'],
    templateUrl: './contents-export-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        TranslatePipe,
    ],
})
export class ContentsExportDialogComponent {
    /** Emits when the dialog is closed or the export has been started. */
    public readonly dialogClose = output();

    /** The schema to export the contents for. */
    public readonly schema = input.required<SchemaDto>();

    /** All languages of the app, to build the paths of localized fields. */
    public readonly languages = input<ReadonlyArray<AppLanguageDto>>([]);

    protected readonly metaPaths = ['id', 'created', 'createdBy', 'lastModified', 'lastModifiedBy', 'status', 'newStatus', 'version'];

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

    protected readonly format = signal<ExportFormat>('Csv');
    protected readonly fields = signal('');
    protected readonly unpublished = signal(false);

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    protected addPath(path: string) {
        const current = this.fields().trim();

        this.fields.set(current ? `${current}, ${path}` : path);
    }

    protected export() {
        const schema = this.schema();
        if (!schema.canContentsExport) {
            return;
        }

        const request = new ExportContentsDto({
            format: this.format(),
            fields: this.fields().trim() || undefined,
            unpublished: this.unpublished(),
        });

        this.schemasState.exportContents(schema, request)
            .subscribe(() => {
                this.dialogClose.emit();
            });
    }
}
