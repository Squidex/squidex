/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Forms from '@angular/forms';

import {
    AppDto,
    AppCreateDto,
    AppsStoreService,
    fadeAnimation
} from 'shared';

const FALLBACK_NAME = 'my-app';

@Ng2.Component({
    selector: 'sqx-app-form',
    template,
    animations: [
        fadeAnimation
    ]
})
export class AppFormComponent implements Ng2.OnInit {
    @Ng2.Input()
    public showClose = false;

    @Ng2.Output()
    public created = new Ng2.EventEmitter<AppDto>();

    @Ng2.Output()
    public cancelled = new Ng2.EventEmitter();

    public creationError = '';
    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Ng2Forms.Validators.required,
                    Ng2Forms.Validators.maxLength(40),
                    Ng2Forms.Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*')
                ]]
        });

    public appName = FALLBACK_NAME;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly formBuilder: Ng2Forms.FormBuilder
    ) {
    }

    public ngOnInit() {
        this.createForm.controls['name'].valueChanges.subscribe(value => {
            this.appName = value;
        });
    }

    public createApp() {
        this.createForm.markAsDirty();

        if (this.createForm.valid) {
            this.createForm.disable();

            const dto = new AppCreateDto(this.createForm.controls['name'].value);

            this.appsStore.createApp(dto)
                .subscribe(app => {
                    this.createForm.reset();
                    this.created.emit(app);
                }, error => {
                    this.reset();
                    this.creationError = error;
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