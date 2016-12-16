/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { fadeAnimation } from 'framework';

import { AppsStoreService } from './../services/apps-store.service';
import { AppDto, CreateAppDto } from './../services/apps.service';

const FALLBACK_NAME = 'my-app';

@Component({
    selector: 'sqx-app-form',
    styleUrls: ['./app-form.component.scss'],
    templateUrl: './app-form.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppFormComponent implements OnInit {
    @Input()
    public showClose = false;

    @Output()
    public created = new EventEmitter<AppDto>();

    @Output()
    public cancelled = new EventEmitter();

    public creationError = '';
    public createForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*')
                ]]
        });

    public appName = FALLBACK_NAME;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.createForm.controls['name'].valueChanges.subscribe(value => {
            this.appName = value;
        });
    }

    public createApp() {
        this.createForm.markAsTouched();

        if (this.createForm.valid) {
            this.createForm.disable();

            const dto = new CreateAppDto(this.createForm.controls['name'].value);

            this.appsStore.createApp(dto)
                .subscribe(app => {
                    this.createForm.reset();
                    this.created.emit(app);
                }, error => {
                    this.reset();
                    this.creationError = error.displayMessage;
                });
        }
    }

    private reset() {
        this.createForm.enable();
        this.creationError = '';
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }
}