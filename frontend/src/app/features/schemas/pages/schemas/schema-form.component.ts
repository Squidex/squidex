/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, output, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';
import { AppsState, CodeEditorComponent, CreateSchemaForm, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FormRowComponent, GenerateSchemaDto, GenerateSchemaForm, LoaderComponent, ModalDialogComponent, SchemaDto, SchemasService, SchemasState, TooltipDirective, TransformInputDirective, TranslatePipe, UIOptions } from '@app/shared';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
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
export class SchemaFormComponent {
    protected readonly hasChatBot = inject(UIOptions).value.canUseChatBot;

    /** Emits the created schema. */
    public readonly create = output<SchemaDto>();

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** The exported schema to clone. Creates an empty schema when not set. */
    public readonly source = input<any>();

    protected readonly sourceActual = linkedSignal(() => this.source());

    protected readonly createForm = new CreateSchemaForm();
    protected readonly generateForm = new GenerateSchemaForm();
    protected readonly generateLog = signal('');

    protected readonly selectedTab = linkedSignal<number>(() => {
        return this.source() ? 1 : 0;
    });

    protected readonly actualForm = computed(() => {
        return this.selectedTab() === 2 ? this.generateForm : this.createForm;
    });

    constructor(
        private readonly appsState: AppsState,
        protected readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
    ) {
        effect(() => {
            this.createForm.load({ type: 'Default', ...this.source(), name: '' });
        });
    }

    protected selectTab(tab: number) {
        this.selectedTab.set(tab);
        this.sourceActual.set(null);

        const { type, name } = this.createForm.form.value;
        this.createForm.load({ type, name });
    }

    private emitCreate(value: SchemaDto) {
        this.create.emit(value);
    }

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected createSchema() {
        if (this.selectedTab() === 2) {
            this.generateCore();
        } else {
            this.createCore();
        }
    }

    protected generatePreview() {
        const value = this.generateForm.submit();
        if (!value) {
            return;
        }

        this.generateLog.set('');

        const dto = new GenerateSchemaDto({ ...value, numberOfContentItems: 20, execute: false });
        this.schemasService.generateSchema(this.appsState.appName, dto)
            .subscribe({
                next: dto => {
                    this.generateLog.set(dto.log.join('\n'));
                    this.generateForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.generateForm.submitFailed(error);
                },
            });
    }

    private generateCore() {
        const value = this.generateForm.submit();
        if (!value || !this.generateLog()) {
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

    private createCore() {
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
