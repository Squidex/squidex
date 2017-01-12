/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import { ValidatorsEx } from 'framework';

import { AppsStoreService } from './../services/apps-store.service';
import { AppDto, CreateAppDto } from './../services/apps.service';

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

    public creationError = '';
    public createFormSubmitted = false;
    public createForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]]
        });

    public appName =
        Observable.of(FALLBACK_NAME)
            .merge(this.createForm.get('name').valueChanges.map(n => n || FALLBACK_NAME));

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public createApp() {
        this.createFormSubmitted = true;

        if (this.createForm.valid) {
            this.createForm.disable();

            const request = new CreateAppDto(this.createForm.get('name').value);

            this.appsStore.createApp(request)
                .subscribe(dto => {
                    this.reset();
                    this.created.emit(dto);
                }, error => {
                    this.createForm.enable();
                    this.creationError = error.displayMessage;
                });
        }
    }

    private reset() {
        this.creationError = '';
        this.createForm.enable();
        this.createFormSubmitted = false;
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }
}