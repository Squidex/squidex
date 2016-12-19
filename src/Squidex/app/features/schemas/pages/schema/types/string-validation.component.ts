/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html'
})
export class StringValidationComponent implements OnDestroy, OnInit {
    private patternSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    public hidePatternMessage: Observable<boolean>;
    public hideDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('maxLength',
            new FormControl());
        this.editForm.addControl('minLength',
            new FormControl());
        this.editForm.addControl('pattern',
            new FormControl());
        this.editForm.addControl('patternMessage',
            new FormControl());
        this.editForm.addControl('defaultValue',
            new FormControl());

        this.hideDefaultValue =
            Observable.of(false)
                .merge(this.editForm.get('isRequired').valueChanges)
                .map(x => !!x);

        this.hidePatternMessage =
            Observable.of(false)
                .merge(this.editForm.get('pattern').valueChanges)
                .map(x => !x || x.trim().length === 0);

        this.patternSubscription =
            this.editForm.get('pattern').valueChanges.subscribe((value: string) => {
                if (!value || value.length === 0) {
                    this.editForm.get('patternMessage').setValue(undefined);
                }
            });
    }

    public ngOnDestroy() {
        this.patternSubscription.unsubscribe();
    }
}