/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ApiUrlConfig, AppsState, CreateAppForm } from '@app/shared/internal';

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
    public template = '';

    public createForm = new CreateAppForm(this.formBuilder);

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsState,
        private readonly formBuilder: FormBuilder,
    ) {
    }

    public emitComplete() {
        this.complete.emit();
    }

    public createApp() {
        const value = this.createForm.submit();

        if (value) {
            const request = { ...value, template: this.template };

            this.appsStore.create(request)
                .subscribe(() => {
                    this.emitComplete();
                }, error => {
                    this.createForm.submitFailed(error);
                });
        }
    }
}
