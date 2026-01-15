/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';
import { ApiUrlConfig, AppsState, CodeEditorComponent, CreateSchemaForm, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FormRowComponent, GenerateSchemaDto, GenerateSchemaForm, LoaderComponent, ModalDialogComponent, SchemaDto, SchemasService, SchemasState, TooltipDirective, TransformInputDirective, TranslatePipe, UIOptions } from '@app/shared';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LoaderComponent,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class SchemaFormComponent implements OnInit {
    public readonly hasChatBot = inject(UIOptions).value.canUseChatBot;

    @Output()
    public create = new EventEmitter<SchemaDto>();

    @Output()
    public dialogClose = new EventEmitter();

    @Input()
    public source: any;

    public createForm = new CreateSchemaForm();
    public generateForm = new GenerateSchemaForm();
    public generateLog: string = '';

    public selectedTab = 0;

    public get actualForm() {
        return this.selectedTab === 2 ? this.generateForm : this.createForm;
    }

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
    ) {
    }

    public ngOnInit() {
        this.createForm.load({ type: 'Default', ...this.source, name: '' });

        if (this.source) {
            this.selectedTab = 1;
        }
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
        this.source = null;

        const { type, name } = this.createForm.form.value;
        this.createForm.load({ type, name });
    }

    public emitCreate(value: SchemaDto) {
        this.create.emit(value);
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createSchema() {
        if (this.selectedTab === 2) {
            this.generateCore();
        } else {
            this.createCore();
        }
    }

    public generatePreview() {
        const value = this.generateForm.submit();
        if (!value) {
            return;
        }

        this.generateLog = '';

        const dto = new GenerateSchemaDto({ ...value, numberOfContentItems: 20, execute: false });
        this.schemasService.generateSchema(this.appsState.appName, dto)
            .subscribe({
                next: dto => {
                    this.generateLog = dto.log.join('\n');
                    this.generateForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.generateForm.submitFailed(error);
                },
            });
    }

    private generateCore() {
        const value = this.generateForm.submit();
        if (!value || !this.generateLog) {
            return;
        }

        const dto = new GenerateSchemaDto({ ...value, numberOfContentItems: 20, execute: true });
        this.schemasService.generateSchema(this.appsState.appName, dto)
            .pipe(
                switchMap(p => this.schemasService.getSchema(this.appsState.appName, p.schemaName!)),
            )
            .subscribe({
                next: dto => {
                    this.schemasState.add(dto);
                    this.emitCreate(dto);
                },
                error: error => {
                    this.createForm.submitFailed(error);
                },
            });

    }

    public createCore() {
        const value = this.createForm.submit();
        if (!value) {
            return;
        }

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
