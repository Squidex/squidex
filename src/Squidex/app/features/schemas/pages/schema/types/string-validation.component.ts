/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { StringFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html'
})
export class StringValidationComponent implements OnDestroy, OnInit {
    private patternSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public hideDefaultValue: Observable<boolean>;
    public hidePatternMessage: Observable<boolean>;

    public ngOnDestroy() {
        this.patternSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.editForm.setControl('maxLength',
            new FormControl(this.properties.maxLength));

        this.editForm.setControl('minLength',
            new FormControl(this.properties.minLength));

        this.editForm.setControl('pattern',
            new FormControl(this.properties.pattern));

        this.editForm.setControl('patternMessage',
            new FormControl(this.properties.patternMessage));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

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
}