/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectorRef, ChangeDetectionStrategy, Component, Host, Input, OnChanges, OnInit, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';
import { Subscription } from 'rxjs';

import { fadeAnimation } from './animations';

const DEFAULT_ERRORS: { [key: string]: string } = {
    required: '{field} is required.',
    pattern: '{field} does not follow the pattern.',
    patternMessage: '{message}',
    minValue: '{field} must be larget than {minValue}.',
    maxValue: '{field} must be larget than {maxValue}.',
    minLength: '{field} must have more than {minLength} characters.',
    maxLength: '{field} cannot have more than {maxLength} characters.',
    validNumber: '{field} is not a valid number.',
    validValues: '{field} is not a valid value.'
};

@Component({
    selector: 'sqx-control-errors',
    styleUrls: ['./control-errors.component.scss'],
    templateUrl: './control-errors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        fadeAnimation
    ]
})
export class ControlErrorsComponent implements OnChanges, OnInit, OnDestroy {
    private formSubscription: Subscription;
    private displayFieldName: string;
    private control: AbstractControl;

    @Input()
    public for: string;

    @Input()
    public fieldName: string;

    @Input()
    public errors: string;

    @Input()
    public submitted: boolean;

    public errorMessages: string[];

    constructor(@Optional() @Host() private readonly form: FormGroupDirective,
        private readonly changeDetector: ChangeDetectorRef
    ) {
        if (!this.form) {
            throw new Error('control-errors must be used with a parent formGroup directive');
        }
    }

    public ngOnChanges() {
        if (this.fieldName) {
            this.displayFieldName = this.fieldName;
        } else if (this.for) {
            this.displayFieldName = this.for.substr(0, 1).toUpperCase() + this.for.substr(1);
        }

        this.update();
    }

    public ngOnDestroy() {
        this.formSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.control = this.form.form.controls[this.for];

        this.formSubscription =
            this.form.form.statusChanges.merge(this.control.statusChanges)
                .subscribe(_ => {
                   this.update();
                });
    }

    private update() {
        if (!this.control) {
            return;
        }

        if (this.control.invalid && (this.control.touched || this.form.form.touched)) {
            const errors: string[] = [];

            for (let key in <any>this.control.errors) {
                if (this.control.errors.hasOwnProperty(key)) {
                    let message: string = (this.errors ? this.errors[key] : null) || DEFAULT_ERRORS[key];
                    let properties = this.control.errors[key];

                    for (let property in properties) {
                        if (properties.hasOwnProperty(property)) {
                            message = message.replace('{' + property + '}', properties[property]);
                        }
                    }

                    message = message.replace('{field}', this.displayFieldName);

                    errors.push(message);
                }
            }

            this.errorMessages = errors.length > 0 ? errors : null;
        } else {
            this.errorMessages = null;
        }

        this.changeDetector.markForCheck();
    }
}