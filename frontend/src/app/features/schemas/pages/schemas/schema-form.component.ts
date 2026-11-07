/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { switchMap, tap } from 'rxjs';
import { AppsState, CodeEditorComponent, CreateSchemaForm, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FormRowComponent, GenerateSchemaDto, GenerateSchemaForm, LoaderComponent, ModalDialogComponent, SchemaDto, SchemasService, SchemasState, SignalSelectDirective, TooltipDirective, TransformInputDirective, TranslatePipe, UIOptions } from '@app/shared';

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
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LoaderComponent,
        ModalDialogComponent,
        SignalSelectDirective,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class SchemaFormComponent {
    private readonly appsState = inject(AppsState);
    private readonly schemasService = inject(SchemasService);
    private readonly uiOptions = inject(UIOptions);

    protected readonly schemasState = inject(SchemasState);

    /** Emits the created schema. */
    public readonly create = output<SchemaDto>();

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** The exported schema to clone. Creates an empty schema when not set. */
    public readonly source = input<any>();

    protected readonly sourceActual = linkedSignal(() => this.source());

    protected readonly generateLog = signal('');

    protected readonly selectedTab = linkedSignal<number>(() => {
        return this.source() ? 1 : 0;
    });

    protected readonly actualForm = computed(() => {
        return this.selectedTab() === 2 ? this.generateForm : this.createForm;
    });

    protected readonly hasChatBot = this.uiOptions.value.canUseChatBot;

    protected readonly createForm = new CreateSchemaForm();

    protected readonly generateForm = new GenerateSchemaForm();

    constructor() {
        effect(() => {
            this.createForm.load({ type: 'Default', ...this.source(), name: '' });
        });
    }

    protected selectTab(tab: number) {
        this.selectedTab.set(tab);
        this.sourceActual.set(null);

        const { type, name } = this.createForm.model();
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
        this.generateForm.submit(value => {
            this.generateLog.set('');

            const request = new GenerateSchemaDto({ ...value, numberOfContentItems: 20, execute: false });

            return this.schemasService.generateSchema(this.appsState.appName, request).pipe(
                tap(dto => this.generateLog.set(dto.log.join('\n'))));
        });
    }

    private generateCore() {
        if (!this.generateLog()) {
            return;
        }

        this.generateForm.submit(value => {
            const request = new GenerateSchemaDto({ ...value, numberOfContentItems: 20, execute: true });

            return this.schemasService.generateSchema(this.appsState.appName, request).pipe(
                switchMap(p => this.schemasService.getSchema(this.appsState.appName, p.schemaName!)),
                tap(dto => {
                    this.schemasState.add(dto);
                    this.emitCreate(dto);
                }));
        });
    }

    private createCore() {
        this.createForm.submit(value => {
            return this.schemasState.create(value).pipe(
                tap(dto => this.emitCreate(dto)));
        });
    }
}
