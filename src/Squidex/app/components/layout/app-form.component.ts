/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Forms from '@angular/forms';

import { Observable } from 'rxjs';

import { fadeAnimation } from './../../framework';

import { AppCreateDto, AppsStoreService } from './../../shared';

const FALLBACK_NAME = 'my-app';

@Ng2.Component({
    selector: 'sqx-app-form',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppFormComponent implements Ng2.OnInit {
    public createForm: Ng2Forms.FormGroup;

    public appName: Observable<string>;

    @Ng2.Input()
    public showClose = false;

    @Ng2.Output()
    public onCreated = new Ng2.EventEmitter();

    @Ng2.Output()
    public onCancelled = new Ng2.EventEmitter();

    public creating = new Ng2.EventEmitter<boolean>();

    public creationError = new Ng2.EventEmitter<any>();

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly formBuilder: Ng2Forms.FormBuilder
    ) {
    }

    public ngOnInit() {
        this.createForm = this.formBuilder.group({
            name: ['', 
                [
                    Ng2Forms.Validators.required,
                    Ng2Forms.Validators.maxLength(40),
                    Ng2Forms.Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*'),
                ]]
        });

        this.appName = this.createForm.controls['name'].valueChanges.map(name => name || FALLBACK_NAME).publishBehavior(FALLBACK_NAME).refCount();
    }

    public submit() {
        if (this.createForm.valid) {
            this.createForm.disable();
            this.creating.emit(true);
            
            const dto = new AppCreateDto(this.createForm.controls['name'].value);

            this.appsStore.createApp(dto)
                .finally(() => {
                    this.reset();
                })
                .subscribe(() => {
                    this.onCreated.emit();
                }, error => {
                    this.creationError.emit(error);
                });
        }
    }

    private reset() {
        this.createForm.enable();
        this.creating.emit(false);
    }

    public cancel() {
        this.onCancelled.emit();
    }
}