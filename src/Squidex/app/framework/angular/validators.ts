/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import {
    AbstractControl,
    ValidatorFn,
    Validators
} from '@angular/forms';

import { DateTime } from './../utils/date-time';

export module ValidatorsEx {
    export function pattern(pattern: string | RegExp, message?: string): ValidatorFn {
        if (!pattern) {
            return Validators.nullValidator;
        }

        let regex: RegExp;
        let regexStr: string;

        if (typeof pattern === 'string') {
            regexStr = `^${pattern}$`;
            regex = new RegExp(regexStr);
        } else {
            regexStr = pattern.toString();
            regex = pattern;
        }

        return (control: AbstractControl): { [key: string]: any } => {
            const n: string = control.value;

            if (n == null || n.length === 0) {
                return {};
            }

            if (!regex.test(n)) {
                if (message) {
                    return { patternmessage: { requiredPattern: regexStr, actualValue: n, message } };
                } else {
                    return { pattern: { requiredPattern: regexStr, actualValue: n } };
                }
            }

            return {};
        };
    }

    export function matchOther(otherControlName: string, message: string) {
        let otherControl: AbstractControl = null;

        return (control: AbstractControl): { [key: string]: any } => {
            if (!control.parent) {
                return {};
            }

            if (otherControl === null) {
                otherControl = control.parent.get(otherControlName) || undefined;

                if (!otherControl) {
                    throw new Error('matchOtherValidator(): other control is not found in parent group');
                }

                otherControl.valueChanges.subscribe(() => {
                    control.updateValueAndValidity({ onlySelf: true });
                });
            }

            if (otherControl && otherControl.value !== control.value) {
                return { matchOther: { message } };
            }

            return {};
        };
    }

    export function validDateTime() {
        return (control: AbstractControl): { [key: string]: any } => {
            const v: string = control.value;

            if (v) {
                try {
                    DateTime.parseISO_UTC(v);
                } catch (e) {
                    return { validdateTime: false };
                }
            }

            return {};
        };
    }

    export function between(minValue?: number, maxValue?: number) {
        if (!minValue || !maxValue) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl): { [key: string]: any } => {
            const n: number = control.value;

            if (typeof n !== 'number') {
                return { validnumber: false };
            } else if (minValue && n < minValue) {
                return { minvalue: { minValue, actualValue: n } };
            } else if (maxValue && n > maxValue) {
                return { maxvalue: { maxValue, actualValue: n } };
            }

            return {};
        };
    }

    export function validValues<T>(values: T[]) {
        if (!values) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl): { [key: string]: any } => {
            const n: T = control.value;

            if (values.indexOf(n) < 0) {
                return { validvalues: false };
            }

            return {};
        };
    }
}