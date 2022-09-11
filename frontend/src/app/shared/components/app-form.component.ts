/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ApiUrlConfig, AppsState, CreateAppForm, TemplateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppFormComponent {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public template?: TemplateDto;

    public createForm = new CreateAppForm();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsState,
    ) {
    }

    public emitComplete() {
        this.complete.emit();
    }

    public createApp() {
        const value = this.createForm.submit();

        if (value) {
            const request = { ...value, template: this.template?.name };

            this.appsStore.create(request)
                .subscribe({
                    next: () => {
                        this.emitComplete();
                    },
                    error: error => {
                        this.createForm.submitFailed(error);
                    },
                });
        }
    }
}
