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
import { Types } from './../utils/types';

export module ValidatorsEx {
    export function pattern(regex: string | RegExp, message?: string): ValidatorFn {
        if (!regex) {
            return Validators.nullValidator;
        }

        let regeExp: RegExp;
        let regexStr: string;

        if (typeof regex === 'string') {
            regexStr = `^${regex}$`;
            regeExp = new RegExp(regexStr);
        } else {
            regexStr = regex.toString();
            regeExp = regex;
        }

        return (control: AbstractControl) => {
            const n: string = control.value;

            if (n == null || n.length === 0) {
                return null;
            }

            if (!regeExp.test(n)) {
                if (message) {
                    return { patternmessage: { requiredPattern: regexStr, actualValue: n, message } };
                } else {
                    return { pattern: { requiredPattern: regexStr, actualValue: n } };
                }
            }

            return null;
        };
    }

    export function match(otherControlName: string, message: string): ValidatorFn {
        let otherControl: AbstractControl | null = null;

        return (control: AbstractControl) => {
            if (!control.parent) {
                return null;
            }

            if (otherControl === null) {
                otherControl = control.parent.get(otherControlName);

                if (!otherControl) {
                    throw new Error('matchValidator(): other control is not found in parent group');
                }

                otherControl.valueChanges.subscribe(() => {
                    control.updateValueAndValidity({ onlySelf: true });
                });
            }

            if (otherControl && otherControl.value !== control.value) {
                return { match: { message } };
            }

            return null;
        };
    }

    export function validDateTime(): ValidatorFn {
        return (control: AbstractControl) => {
            const v: string = control.value;

            if (v) {
                try {
                    DateTime.parseISO_UTC(v);
                } catch (e) {
                    return { validdatetime: false };
                }
            }

            return null;
        };
    }

    export function between(minValue?: number, maxValue?: number): ValidatorFn {
        if (!minValue || !maxValue) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl) => {
            const value: number = control.value;

            if (!Types.isNumber(value)) {
                return { validnumber: false };
            } else if (minValue && value < minValue) {
                return { minvalue: { minValue, actualValue: value } };
            } else if (maxValue && value > maxValue) {
                return { maxvalue: { maxValue, actualValue: value } };
            }

            return null;
        };
    }

    export function validValues<T>(values: T[]): ValidatorFn {
        if (!values) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl) => {
            const n: T = control.value;

            if (values.indexOf(n) < 0) {
                return { validvalues: false };
            }

            return null;
        };
    }

    export function noop(): ValidatorFn {
        return (control: AbstractControl) => {
            return null;
        };
    }
}