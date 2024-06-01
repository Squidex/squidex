/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApiUrlConfig, AppsState, CodeEditorComponent, ControlErrorsComponent, CreateSchemaForm, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, ModalDialogComponent, SchemaDto, SchemasState, TooltipDirective, TransformInputDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        ControlErrorsComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class SchemaFormComponent implements OnInit {
    @Output()
    public create = new EventEmitter<SchemaDto>();

    @Output()
    public dialogClose = new EventEmitter();

    @Input()
    public import: any;

    public createForm = new CreateSchemaForm();

    public showImport = false;

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
    ) {
    }

    public ngOnInit() {
        this.createForm.load({ type: 'Default', ...this.import, name: '' });

        this.showImport = !!this.import;
    }

    public toggleImport() {
        this.showImport = !this.showImport;

        return false;
    }

    public emitCreate(value: SchemaDto) {
        this.create.emit(value);
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createSchema() {
        const value = this.createForm.submit();

        if (value) {
            this.schemasState.create(value)
                .subscribe({
                    next: dto => {
                        this.emitCreate(dto);
                    },
                    error: error => {
                        this.createForm.submitFailed(error);
                    },
                });
        }
    }
}
