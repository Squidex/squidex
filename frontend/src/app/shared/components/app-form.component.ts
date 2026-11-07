/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, linkedSignal, output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApiUrlConfig, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FormRowComponent, MarkdownDirective, ModalDialogComponent, TooltipDirective, TransformInputDirective, TranslatePipe } from '@app/framework';
import { AppsState, CreateAppDto, CreateAppForm, TemplateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class AppFormComponent {
    /** Emits when the dialog is closed or the app has been created. */
    public readonly dialogClose = output();

    /** The template that is selected initially. Creates a blank app when not set. */
    public readonly template = input<TemplateDto>();

    /** The templates to choose from. */
    public readonly templates = input.required<TemplateDto[]>();

    public readonly templateSelected = linkedSignal(() => this.template());

    public readonly createForm = new CreateAppForm();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsState,
    ) {
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public selectTemplate(template?: TemplateDto) {
        this.templateSelected.set(template);
    }

    public createApp() {
        const value = this.createForm.submit();
        if (!value) {
            return;
        }

        const request = new CreateAppDto({ ...value, template: this.templateSelected()?.name });

        this.appsStore.create(request)
            .subscribe({
                next: () => {
                    this.emitClose();
                },
                error: error => {
                    this.createForm.submitFailed(error);
                },
            });
    }
}
