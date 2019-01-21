/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, ValidatorFn, Validators } from '@angular/forms';

import { DateTime } from '@app/framework/internal';

function isEmptyInputValue(value: any): boolean {
    return value == null || value.length === 0;
}

export module ValidatorsEx {
    export function pattern(regex: string | RegExp, message?: string): ValidatorFn {
        if (!regex) {
            return Validators.nullValidator;
        }

        const inner = Validators.pattern(regex);

        return (control: AbstractControl) => {
            const error = inner(control);

            if (error !== null && error.pattern && message) {
                return { patternmessage: { requiredPattern: error.pattern.requiredPattern, actualValue: error.pattern.actualValue, message } };
            }

            return error;
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

    export function between(min?: number, max?: number): ValidatorFn {
        if (!min && !max) {
            return Validators.nullValidator;
        }

        if (max && min) {
            return (control: AbstractControl) => {
                if (isEmptyInputValue(control.value)) {
                    return null;
                }

                const value = parseFloat(control.value);

                if (min === max) {
                    if (isNaN(value) || value !== min) {
                        return { exactly: { expected: min, actual: value } };
                    }
                } else {
                    if (isNaN(value) || value < min || value > max) {
                        return { between: { min: min, max: max, actual: value }};
                    }
                }

                return null;
            };
        } else if (max) {
            return Validators.max(max);
        } else {
            return Validators.min(min!);
        }
    }

    export function betweenLength(minLength?: number, maxLength?: number): ValidatorFn {
        if (!minLength && !maxLength) {
            return Validators.nullValidator;
        }

        if (maxLength && minLength) {
            return (control: AbstractControl) => {
                if (isEmptyInputValue(control.value)) {
                    return null;
                }

                const length: number = control.value ? control.value.length : 0;

                if (minLength === maxLength) {
                    if (isNaN(length) || length !== minLength) {
                        return { exactlylength: { expected: minLength, actual: length } };
                    }
                } else {
                    if (isNaN(length) || length < minLength || length > maxLength) {
                        return { betweenlength: { minLength, maxLength, actual: length }};
                    }
                }

                return null;
            };
        } else if (maxLength) {
            return Validators.maxLength(maxLength);
        } else {
            return Validators.minLength(minLength!);
        }
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

    export function validArrayValues<T>(values: T[]): ValidatorFn {
        if (!values) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl) => {
            const ns: T[] = control.value;

            if (ns) {
                for (let n of ns) {
                    if (values.indexOf(n) < 0) {
                        return { validarrayvalues: { invalidvalue: n } };
                    }
                }
            }

            return null;
        };
    }
}