/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, ValidatorFn, Validators } from '@angular/forms';
import { DateTime, Types } from '@app/framework/internal';

function isEmptyInputValue(value: any): boolean {
    return value == null || value === undefined || value.length === 0;
}

export module ValidatorsEx {
    export function pattern(regex: string | RegExp, message?: string): ValidatorFn {
        if (!regex) {
            return Validators.nullValidator;
        }

        const inner = Validators.pattern(regex);

        return (control: AbstractControl) => {
            const error = inner(control);

            if (!!error && error.pattern && message) {
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
                    DateTime.parseISO(v);
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
                    if (!Types.isNumber(value) || Number.isNaN(value) || value !== min) {
                        return { exactly: { expected: min, actual: value } };
                    }
                } else if (!Types.isNumber(value) || Number.isNaN(value) || value < min || value > max) {
                    return { between: { min, max, actual: value } };
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

                const length: number = control.value?.length || 0;

                if (minLength === maxLength) {
                    if (!Types.isNumber(length) || Number.isNaN(length) || length !== minLength) {
                        return { exactlylength: { expected: minLength, actual: length } };
                    }
                } else if (!Types.isNumber(length) || Number.isNaN(length) || length < minLength || length > maxLength) {
                    return { betweenlength: { minlength: minLength, maxlength: maxLength, actual: length } };
                }

                return null;
            };
        } else if (maxLength) {
            return Validators.maxLength(maxLength);
        } else {
            return Validators.minLength(minLength!);
        }
    }

    export function validValues<T>(allowed: ReadonlyArray<T>): ValidatorFn {
        if (!allowed || allowed.length === 0) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl) => {
            const value: T = control.value;

            if (allowed.indexOf(value) < 0) {
                return { validvalues: false };
            }

            return null;
        };
    }

    export function validArrayValues<T>(allowed: ReadonlyArray<T>): ValidatorFn {
        if (!allowed || allowed.length === 0) {
            return Validators.nullValidator;
        }

        return (control: AbstractControl) => {
            const values: T[] = control.value;

            if (values) {
                for (const value of values) {
                    if (allowed.indexOf(value) < 0) {
                        return { validarrayvalues: { invalidvalue: value } };
                    }
                }
            }

            return null;
        };
    }

    export function uniqueStrings(): ValidatorFn {
        return (control: AbstractControl) => {
            if (isEmptyInputValue(control.value) || !Types.isArrayOfString(control.value)) {
                return null;
            }

            const values: string[] = control.value;
            const valuesUnique: { [key: string]: boolean } = {};

            for (const value of values) {
                if (valuesUnique[value]) {
                    return { uniquestrings: false };
                } else {
                    valuesUnique[value] = true;
                }
            }

            return null;
        };
    }

    export function uniqueObjectValues(fields: ReadonlyArray<string>): ValidatorFn {
        return (control: AbstractControl) => {
            if (isEmptyInputValue(control.value) || !Types.isArrayOfObject(control.value)) {
                return null;
            }

            const items: any[] = control.value;

            const duplicateKeys: object = {};

            for (const field of fields) {
                const values: any[] = [];

                for (const item of items) {
                    if (item.hasOwnProperty(field)) {
                        const fieldValue = item[field];

                        for (const other of values) {
                            if (Types.equals(other, fieldValue)) {
                                duplicateKeys[field] = true;
                                break;
                            }
                        }

                        values.push(fieldValue);
                    }
                }
            }

            const keys = Object.keys(duplicateKeys);

            if (keys.length > 0) {
                return { uniqueobjectvalues: { fields: keys.join(', ') } };
            }

            return null;
        };
    }
}
