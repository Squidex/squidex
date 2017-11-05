/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { ApiUrlConfig, ValidatorsEx } from 'framework';

import {
    AppDto,
    AppsStoreService,
    CreateAppDto
} from './../declarations-base';

const FALLBACK_NAME = 'my-app';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html'
})
export class AppFormComponent {
    @Output()
    public created = new EventEmitter<AppDto>();

    @Output()
    public cancelled = new EventEmitter();

    public createFormError? = '';
    public createFormSubmitted = false;
    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]]
        });

    public appName =
        this.createForm.controls['name'].valueChanges.map(n => n || FALLBACK_NAME)
            .startWith(FALLBACK_NAME);

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly appsStore: AppsStoreService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public cancel() {
        this.emitCancelled();
        this.resetCreateForm();
    }

    public createApp() {
        this.createFormSubmitted = true;

        if (this.createForm.valid) {
            this.createForm.disable();

            const request = new CreateAppDto(this.createForm.controls['name'].value);

            this.appsStore.createApp(request)
                .subscribe(dto => {
                    this.resetCreateForm();
                    this.emitCreated(dto);
                }, error => {
                    this.enableCreateForm(error.displayMessage);
                });
        }
    }

    private emitCancelled() {
        this.cancelled.emit();
    }

    private emitCreated(app: AppDto) {
        this.created.emit(app);
    }

    private enableCreateForm(message: string) {
        this.createForm.enable();
        this.createFormSubmitted = false;
        this.createFormError = message;
    }

    private resetCreateForm() {
        this.createFormError = '';
        this.createForm.enable();
        this.createFormSubmitted = false;
    }
}