/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, input, linkedSignal, output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FormRowComponent, MarkdownDirective, ModalDialogComponent, TooltipDirective, TransformInputDirective, TranslatePipe } from '@app/framework';
import { AppsState, CreateAppDto, CreateAppForm, TemplateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
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

    protected createApp() {
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
