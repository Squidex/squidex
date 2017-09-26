/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { Component, Host, Input, OnChanges, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';

import { fadeAnimation } from './animations';

const DEFAULT_ERRORS: { [key: string]: string } = {
    required: '{field} is required.',
    pattern: '{field} does not follow the pattern.',
    patternmessage: '{message}',
    minvalue: '{field} must be larger than {minValue}.',
    maxvalue: '{field} must be smaller than {maxValue}.',
    minlength: '{field} must have a length of more than {requiredLength}.',
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
    ]
})
export class ControlErrorsComponent implements OnChanges {
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

    @Input()
    public submitOnly = false;

    public get errorMessages(): string[] | null {
        if (!this.control) {
            return null;
        }

        if (this.control.invalid && ((this.control.touched && !this.submitOnly) || this.submitted) && this.control.errors) {
            const errors: string[] = [];

            for (let key in <any>this.control.errors) {
                if (this.control.errors.hasOwnProperty(key)) {
                    let message = (this.errors ? this.errors[key] : null) || DEFAULT_ERRORS[key];

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

            return errors.length > 0 ? errors : null;
        }

        return null;
    }

    constructor(
        @Optional() @Host() private readonly formGroupDirective: FormGroupDirective
    ) {
        if (!this.formGroupDirective) {
            throw new Error('control-errors must be used with a parent formGroup directive');
        }
    }

    public ngOnChanges() {
        if (this.fieldName) {
            this.displayFieldName = this.fieldName;
        } else if (this.for) {
            this.displayFieldName = this.for.substr(0, 1).toUpperCase() + this.for.substr(1);
        }

        this.control = this.formGroupDirective.form.controls[this.for];
    }
}