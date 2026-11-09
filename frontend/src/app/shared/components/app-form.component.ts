/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FormRowComponent, MarkdownDirective, ModalDialogComponent, TextInputDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppsState, CreateAppDto, CreateAppForm, TemplateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html',
    imports: [
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        TextInputDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AppFormComponent {
    private readonly appsStore = inject(AppsState);

    /** Emits when the dialog is closed or the app has been created. */
    public readonly dialogClose = output();

    /** The template that is selected initially. Creates a blank app when not set. */
    public readonly template = input<TemplateDto>();

    /** The templates to choose from. */
    public readonly templates = input.required<TemplateDto[]>();

    protected readonly templateSelected = linkedSignal(() => this.template());

    protected readonly createForm = new CreateAppForm();

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected selectTemplate(template?: TemplateDto) {
        this.templateSelected.set(template);
    }

    protected async createApp() {
        const isCreated = await this.createForm.submit(value => {
            const request = new CreateAppDto({ ...value, template: this.templateSelected()?.name });

            return this.appsStore.create(request);
        });

        if (isCreated) {
            this.emitClose();
        }
    }
}
