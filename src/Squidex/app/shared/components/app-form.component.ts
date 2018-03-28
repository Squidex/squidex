/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ApiUrlConfig,
    AppsState,
    CreateAppDto,
    ValidatorsEx
} from '@app/shared/internal';

const FALLBACK_NAME = 'my-app';

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

    public createFormError = '';
    public createFormSubmitted = false;
    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]
            ]
        });

    public appName =
        this.createForm.controls['name'].valueChanges.map(n => n || FALLBACK_NAME)
            .startWith(FALLBACK_NAME);

    constructor(public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public complete() {
        this.completed.emit();
    }

    public createApp() {
        this.createFormSubmitted = true;

        if (this.createForm.valid) {
            this.createForm.disable();

            const request = new CreateAppDto(this.createForm.controls['name'].value, this.template);

            this.appsStore.createApp(request)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.enableCreateForm(error.displayMessage);
                });
        }
    }

    private enableCreateForm(message: string) {
        this.createForm.enable();
        this.createFormSubmitted = false;
        this.createFormError = message;
    }
}