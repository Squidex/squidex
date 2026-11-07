/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime, Types } from '@app/framework/internal';

export type ValueError = { readonly kind: string } & Record<string, any>;

export type ValueValidator = (value: any) => ValueError | null;

function isEmptyInputValue(value: any): boolean {
    return value == null || value === undefined || value.length === 0;
}

export module ValidatorsEx {
    export const nullValidator: ValueValidator = () => null;

    export function required(): ValueValidator {
        return value => {
            if (isEmptyInputValue(value)) {
                return { kind: 'required' };
            }

            return null;
        };
    }

    export function min(minValue: number): ValueValidator {
        return value => {
            if (isEmptyInputValue(value)) {
                return null;
            }

            const actual = parseFloat(value);
            if (!Number.isNaN(actual) && actual < minValue) {
                return { kind: 'min', min: minValue, actual };
            }

            return null;
        };
    }

    export function max(maxValue: number): ValueValidator {
        return value => {
            if (isEmptyInputValue(value)) {
                return null;
            }

            const actual = parseFloat(value);
            if (!Number.isNaN(actual) && actual > maxValue) {
                return { kind: 'max', max: maxValue, actual };
            }

            return null;
        };
    }

    export function minLength(requiredLength: number): ValueValidator {
        return value => {
            if (isEmptyInputValue(value) || !Types.isNumber(value.length)) {
                return null;
            }

            if (value.length < requiredLength) {
                return { kind: 'minlength', requiredLength, actualLength: value.length };
            }

            return null;
        };
    }

    export function maxLength(requiredLength: number): ValueValidator {
        return value => {
            if (isEmptyInputValue(value) || !Types.isNumber(value.length)) {
                return null;
            }

            if (value.length > requiredLength) {
                return { kind: 'maxlength', requiredLength, actualLength: value.length };
            }

            return null;
        };
    }

    export function pattern(regex: string | RegExp, message?: string): ValueValidator {
        if (!regex) {
            return nullValidator;
        }

        let requiredPattern: string;
        let compiled: RegExp;

        if (Types.isString(regex)) {
            requiredPattern = regex;

            if (!requiredPattern.startsWith('^')) {
                requiredPattern = `^${requiredPattern}`;
            }

            if (!requiredPattern.endsWith('$')) {
                requiredPattern = `${requiredPattern}$`;
            }

            compiled = new RegExp(requiredPattern);
        } else {
            requiredPattern = regex.toString();
            compiled = regex;
        }

        return value => {
            if (isEmptyInputValue(value)) {
                return null;
            }

            if (compiled.test(value)) {
                return null;
            }

            if (message) {
                return { kind: 'patternmessage', requiredPattern, actualValue: value, message };
            }

            return { kind: 'pattern', requiredPattern, actualValue: value };
        };
    }

    export function validDateTime(): ValueValidator {
        return value => {
            if (!value) {
                return null;
            }

            try {
                DateTime.parseISO(value);
                return null;
            } catch {
                return { kind: 'validdatetime' };
            }
        };
    }

    export function between(minValue?: number, maxValue?: number): ValueValidator {
        if (!minValue && !maxValue) {
            return nullValidator;
        }

        if (!maxValue) {
            return min(minValue!);
        }

        if (!minValue) {
            return max(maxValue);
        }

        return value => {
            if (isEmptyInputValue(value)) {
                return null;
            }

            const actual = parseFloat(value);

            if (minValue === maxValue) {
                if (Number.isNaN(actual) || actual !== minValue) {
                    return { kind: 'exactly', expected: minValue, actual };
                }
            } else if (Number.isNaN(actual) || actual < minValue || actual > maxValue) {
                return { kind: 'between', min: minValue, max: maxValue, actual };
            }

            return null;
        };
    }

    export function betweenLength(minValue?: number, maxValue?: number): ValueValidator {
        if (!minValue && !maxValue) {
            return nullValidator;
        }

        if (!maxValue) {
            return minLength(minValue!);
        }

        if (!minValue) {
            return maxLength(maxValue);
        }

        return value => {
            if (isEmptyInputValue(value)) {
                return null;
            }

            const actual: number = value?.length || 0;

            if (minValue === maxValue) {
                if (actual !== minValue) {
                    return { kind: 'exactlylength', expected: minValue, actual };
                }
            } else if (actual < minValue || actual > maxValue) {
                return { kind: 'betweenlength', minlength: minValue, maxlength: maxValue, actual };
            }

            return null;
        };
    }

    export function validValues<T>(allowed: ReadonlyArray<T>): ValueValidator {
        if (!allowed || allowed.length === 0) {
            return nullValidator;
        }

        return value => {
            if (!allowed.includes(value)) {
                return { kind: 'validvalues' };
            }

            return null;
        };
    }

    export function validArrayValues<T>(allowed: ReadonlyArray<T>): ValueValidator {
        if (!allowed || allowed.length === 0) {
            return nullValidator;
        }

        return value => {
            if (!Types.isArray(value)) {
                return null;
            }

            for (const item of value) {
                if (allowed.indexOf(item) < 0) {
                    return { kind: 'validarrayvalues', invalidvalue: item };
                }
            }

            return null;
        };
    }

    export function uniqueStrings(): ValueValidator {
        return value => {
            if (isEmptyInputValue(value) || !Types.isArrayOfString(value)) {
                return null;
            }

            const valuesUnique: { [key: string]: boolean } = {};

            for (const item of value) {
                if (valuesUnique[item]) {
                    return { kind: 'uniquestrings' };
                }

                valuesUnique[item] = true;
            }

            return null;
        };
    }

    export function uniqueObjectValues(fields: ReadonlyArray<string>): ValueValidator {
        return value => {
            if (isEmptyInputValue(value) || !Types.isArrayOfObject(value)) {
                return null;
            }

            const duplicateKeys = {} as Record<string, boolean>;

            for (const field of fields) {
                const values: any[] = [];

                for (const item of value as Record<string, any>[]) {
                    if (!item.hasOwnProperty(field)) {
                        continue;
                    }

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

            const keys = Object.keys(duplicateKeys);
            if (keys.length > 0) {
                return { kind: 'uniqueobjectvalues', fields: keys.join(', ') };
            }

            return null;
        };
    }
}

export function validateValue(validators: ReadonlyArray<ValueValidator>, value: any) {
    const result: ValueError[] = [];

    for (const validator of validators) {
        const error = validator(value);

        if (error) {
            result.push(error);
        }
    }

    return result;
}
