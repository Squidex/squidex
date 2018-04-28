/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    ApiUrlConfig,
    AppsState,
    CreateAppDto,
    CreateAppForm
} from '@app/shared/internal';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html'
})
export class AppFormComponent {
    @Output()
    public completed = new EventEmitter();

    @Input()
    public template = '';

    public createForm = new CreateAppForm(this.formBuilder);

    constructor(public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public complete() {
        this.completed.emit();
    }

    public createApp() {
        const value = this.createForm.submit();

        if (value) {
            const request = new CreateAppDto(value.name, this.template);

            this.appsStore.create(request)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.createForm.submitFailed(error);
                });
        }
    }
}