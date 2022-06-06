/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module Types {
    export function isString(value: any): value is string {
        return typeof value === 'string' || value instanceof String;
    }

    export function isNumber(value: any): value is number {
        return typeof value === 'number' && Number.isFinite(value);
    }

    export function isArray(value: any): value is Array<any> {
        return Array.isArray(value);
    }

    export function isFunction(value: any): value is Function {
        return typeof value === 'function';
    }

    export function isObject(value: any): value is Object {
        return value && typeof value === 'object' && value.constructor === Object;
    }

    export function isBoolean(value: any): value is boolean {
        return typeof value === 'boolean';
    }

    export function isNull(value: any): value is null {
        return value === null;
    }

    export function isUndefined(value: any): value is undefined {
        return typeof value === 'undefined';
    }

    export function isRegExp(value: any): value is RegExp {
        return value && typeof value === 'object' && value.constructor === RegExp;
    }

    export function isDate(value: any): value is Date {
        return value instanceof Date;
    }

    export function is<TClass>(x: any, c: new (...args: any[]) => TClass): x is TClass {
        return x instanceof c;
    }

    export function isArrayOfNumber(value: any): value is Array<number> {
        return isArrayOf(value, isNumber);
    }

    export function isArrayOfObject(value: any): value is Array<Object> {
        return isArrayOf(value, isObject);
    }

    export function isArrayOfString(value: any): value is Array<string> {
        return isArrayOf(value, isString);
    }

    export function isArrayOf(value: any, validator: (v: any) => boolean): boolean {
        if (!Array.isArray(value)) {
            return false;
        }

        for (const v of value) {
            if (!validator(v)) {
                return false;
            }
        }

        return true;
    }

    export function isEmpty(value: any): boolean {
        if (Types.isArray(value)) {
            for (const v of value) {
                if (!isEmpty(v)) {
                    return false;
                }
            }

            return true;
        }

        if (Types.isObject(value)) {
            for (const item of Object.values(value)) {
                if (!isEmpty(item)) {
                    return false;
                }
            }

            return true;
        }

        // eslint-disable-next-line @typescript-eslint/no-unnecessary-boolean-literal-compare
        return Types.isUndefined(value) === true || Types.isNull(value) === true;
    }

    export function fastMerge<T>(lhs: ReadonlyArray<T>, rhs: ReadonlyArray<T>) {
        if (rhs.length === 0) {
            return lhs;
        }

        if (lhs.length === 0) {
            return rhs;
        }

        return [...lhs, ...rhs];
    }

    export function clone<T>(lhs: T): T {
        if (Types.isArray(lhs)) {
            const result = [];

            for (let i = 0; i < lhs.length; i++) {
                result[i] = clone(lhs[i]);
            }

            return result as any;
        } else if (Types.isObject(lhs)) {
            const result = {};

            for (const [key, value] of Object.entries(lhs)) {
                result[key] = clone(value);
            }

            return result as any;
        }

        return lhs;
    }

    export function equals(lhs: any, rhs: any, lazyString = false) {
        // eslint-disable-next-line no-self-compare
        if (lhs === rhs || (lhs !== lhs && rhs !== rhs)) {
            return true;
        }

        if (lazyString) {
            const result =
                (lhs === '' && Types.isUndefined(rhs) ||
                (rhs === '' && Types.isUndefined(lhs)));

            if (result) {
                return true;
            }
        }

        if (!lhs || !rhs) {
            return false;
        }

        if (Types.isArray(lhs) && Types.isArray(rhs)) {
            if (lhs.length !== rhs.length) {
                return false;
            }

            for (let i = 0; i < lhs.length; i++) {
                if (!equals(lhs[i], rhs[i], lazyString)) {
                    return false;
                }
            }

            return true;
        } else if (Types.isObject(lhs) && Types.isObject(rhs)) {
            const lhsKeys = Object.keys(lhs);

            if (lhsKeys.length !== Object.keys(rhs).length) {
                return false;
            }

            for (const key of lhsKeys) {
                if (!equals(lhs[key], rhs[key], lazyString)) {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    export function mergeInto(target: {}, source: {} | undefined | null) {
        if (!Types.isObject(target) || !Types.isObject(source)) {
            return source;
        }

        Object.entries(source).forEach(([key, sourceValue]) => {
            const targetValue = target[key];

            if (Types.isArray(targetValue) && Types.isArray(sourceValue)) {
                target[key] = targetValue.concat(sourceValue);
            } else if (Types.isObject(targetValue) && Types.isObject(sourceValue)) {
                target[key] = mergeInto({ ...targetValue }, sourceValue);
            } else {
                target[key] = sourceValue;
            }
        });

        return target;
    }

    export function booleanToString(value: boolean) {
        return value ? 'Yes' : 'No';
    }

    export function objectToString(value: Object, exclude?: string[], maxLength = 100) {
        let result = '';

        for (const [key, property] of Object.entries(value)) {
            let formatted = '';

            if (exclude && exclude.indexOf(key) >= 0) {
                continue;
            }

            if (Types.isString(property)) {
                formatted = property;
            } else if (Types.isNumber(property)) {
                formatted = `${property}`;
            } else if (Types.isBoolean(property)) {
                formatted = booleanToString(property);
            }

            if (formatted) {
                if (result.length > 0) {
                    result += ', ';
                }

                result += formatted;
            }

            if (result.length >= maxLength) {
                break;
            }
        }

        return result;
    }
}
