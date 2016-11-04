/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Forms from '@angular/forms';

import { BehaviorSubject } from 'rxjs';

import { 
    AppCreateDto, 
    AppsStoreService,
    fadeAnimation 
} from 'shared';

const FALLBACK_NAME = 'my-app';

@Ng2.Component({
    selector: 'sqx-app-form',
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppFormComponent {
    @Ng2.Input()
    public showClose = false;

    @Ng2.Output()
    public created = new Ng2.EventEmitter();

    @Ng2.Output()
    public cancelled = new Ng2.EventEmitter();

    public createForm =
        this.formBuilder.group({
            name: ['', 
                [
                    Ng2Forms.Validators.required,
                    Ng2Forms.Validators.maxLength(40),
                    Ng2Forms.Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*'),
                ]]
        });

    public appName = 
        this.createForm.controls['name'].valueChanges.map(name => name || FALLBACK_NAME).publishBehavior(FALLBACK_NAME).refCount();

    public creating = 
        new BehaviorSubject<boolean>(false);

    public creationError = 
        new BehaviorSubject<string>('');

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly formBuilder: Ng2Forms.FormBuilder
    ) {
    }

    public submit() {
        this.createForm.markAsDirty();

        if (this.createForm.valid) {
            this.createForm.disable();
            this.creating.next(true);
            
            const dto = new AppCreateDto(this.createForm.controls['name'].value);

            this.appsStore.createApp(dto)
                .subscribe(() => {
                    this.createForm.reset();
                    this.created.emit();
                }, error => {
                    this.reset();
                    this.creationError.next(error);
                });
        }
    }

    private reset() {
        this.createForm.enable();
        this.creating.next(false);
        this.creationError.next(null);
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }
}