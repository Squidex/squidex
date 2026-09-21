/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppLanguageDto, ExportContentsDto, ExportFormat, FormRowComponent, MarkdownDirective, ModalDialogComponent, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';

type FieldPaths = { label: string; paths: ReadonlyArray<string> };

@Component({
    selector: 'sqx-contents-export-dialog',
    styleUrls: ['./contents-export-dialog.component.scss'],
    templateUrl: './contents-export-dialog.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        TranslatePipe,
    ],
})
export class ContentsExportDialogComponent implements OnInit {
    @Output()
    public dialogClose = new EventEmitter();

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto> = [];

    public readonly metaPaths = ['id', 'created', 'createdBy', 'lastModified', 'lastModifiedBy', 'status', 'newStatus', 'version'];

    public fieldPaths: ReadonlyArray<FieldPaths> = [];

    public format: ExportFormat = 'Csv';
    public fields = '';
    public unpublished = false;

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnInit() {
        this.fieldPaths = this.schema.fields.filter(x => x.properties.isContentField).map(field => {
            const paths =
                field.isLocalizable ?
                this.languages.map(x => `data.${field.name}.${x.iso2Code}`) :
                [`data.${field.name}`];

            return { label: field.displayName, paths };
        });
    }

    public addPath(path: string) {
        const current = this.fields.trim();

        this.fields = current ? `${current}, ${path}` : path;
    }

    public export() {
        if (!this.schema.canContentsExport) {
            return;
        }

        const request = new ExportContentsDto({
            format: this.format,
            fields: this.fields.trim() || undefined,
            unpublished: this.unpublished,
        });

        this.schemasState.exportContents(this.schema, request)
            .subscribe(() => {
                this.dialogClose.emit();
            });
    }
}
