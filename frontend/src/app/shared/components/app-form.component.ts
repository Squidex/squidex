/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApiUrlConfig, ControlErrorsComponent, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, ModalDialogComponent, TooltipDirective, TransformInputDirective, TranslatePipe } from '@app/framework';
import { AppsState, CreateAppForm, TemplateDto } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
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
export class AppFormComponent {
    @Output()
    public dialogClose = new EventEmitter();

    @Input()
    public template?: TemplateDto;

    public createForm = new CreateAppForm();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsState,
    ) {
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createApp() {
        const value = this.createForm.submit();

        if (value) {
            const request = { ...value, template: this.template?.name };

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
}
