/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Host, Input, OnChanges, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { fadeAnimation } from './animations';

const DEFAULT_ERRORS: { [key: string]: string } = {
    required: '{field} is required.',
    pattern: '{field} does not follow the pattern.',
    patternmessage: '{message}',
    minvalue: '{field} must be larger than {minValue}.',
    maxvalue: '{field} must be smaller than {maxValue}.',
    minmax: '{field} must have a length of more than {requiredLength}.',
    maxlength: '{field} must have a length of less than {requiredLength}.',
    match: '{message}',
    validdatetime: '{field} is not a valid date time',
    validnumber: '{field} is not a valid number.',
    validvalues: '{field} is not a valid value.'
};

@Component({
    selector: 'sqx-control-errors',
    styleUrls: ['./control-errors.component.scss'],
    templateUrl: './control-errors.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ControlErrorsComponent implements OnChanges, OnDestroy {
    private displayFieldName: string;
    private control: AbstractControl;
    private controlSubscription: Subscription | null = null;
    private originalMarkAsTouched: any;

    @Input()
    public for: string;

    @Input()
    public fieldName: string;

    @Input()
    public errors: string;

    @Input()
    public submitted: boolean;

    @Input()
    public submitOnly = false;

    public errorMessages: string[] = [];

    constructor(
        @Optional() @Host() private readonly formGroupDirective: FormGroupDirective,
        private readonly changeDetector: ChangeDetectorRef
    ) {
        if (!this.formGroupDirective) {
            throw new Error('control-errors must be used with a parent formGroup directive');
        }
    }

    public ngOnDestroy() {
        this.unsubscribe();
    }

    public ngOnChanges() {
        if (this.fieldName) {
            this.displayFieldName = this.fieldName;
        } else if (this.for) {
            this.displayFieldName = this.for.substr(0, 1).toUpperCase() + this.for.substr(1);
        }

        const control = this.formGroupDirective.form.controls[this.for];

        if (this.control !== control) {
            this.unsubscribe();

            this.control = control;

            if (control) {
                const self = this;

                this.controlSubscription =
                    Observable.merge(control.valueChanges, control.statusChanges)
                        .subscribe(() => {
                            this.createMessages();
                        });

                this.originalMarkAsTouched = this.control.markAsTouched;

                this.control['markAsTouched'] = function () {
                    self.originalMarkAsTouched.apply(this, arguments);

                    self.createMessages();
                };
            }
        }

        this.createMessages();
    }

    private unsubscribe() {
        if (this.controlSubscription) {
            this.controlSubscription.unsubscribe();
        }

        if (this.control && this.originalMarkAsTouched) {
            this.control['markAsTouched'] = this.originalMarkAsTouched;
        }
    }

    private createMessages() {
        const errors: string[] = [];

        if (this.control.invalid && ((this.control.touched && !this.submitOnly) || this.submitted) && this.control.errors) {
            for (let key in <any>this.control.errors) {
                if (this.control.errors.hasOwnProperty(key)) {
                    let message = (this.errors ? this.errors[key] : null) || DEFAULT_ERRORS[key.toLowerCase()];

                    if (!message) {
                        continue;
                    }

                    const properties = this.control.errors[key];

                    for (let property in properties) {
                        if (properties.hasOwnProperty(property)) {
                            message = message.replace(`{${property}}`, properties[property]);
                        }
                    }

                    message = message.replace('{field}', this.displayFieldName);

                    errors.push(message);
                }
            }
        }

        this.errorMessages = errors;

        this.changeDetector.detectChanges();
    }
}