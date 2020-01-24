/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

export module Types {
    export function isString(value: any): value is string {
        return typeof value === 'string' || value instanceof String;
    }

    export function isNumber(value: any): value is number {
        return typeof value === 'number' && isFinite(value);
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
        return isArrayOf(value, v => isNumber(v));
    }

    export function isArrayOfString(value: any): value is Array<string> {
        return isArrayOf(value, v => isString(v));
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
            for (const key in value) {
                if (value.hasOwnProperty(key)) {
                    if (!isEmpty(value[key])) {
                        return false;
                    }
                }
            }

            return true;
        }

        return Types.isUndefined(value) === true || Types.isNull(value) === true;
    }

    export function clone<T>(lhs: T): T {
        const any: any = lhs;

        if (Types.isArray(lhs)) {
            const result = [];

            for (let i = 0; i < lhs.length; i++) {
                result[i] = clone(lhs[i]);
            }

            return result as any;
        } else if (Types.isObject(lhs)) {
            const result = {};

            for (let key in any) {
                if (any.hasOwnProperty(key)) {
                    result[key] = clone(lhs[key]);
                }
            }

            return result as any;
        }

        return lhs;
    }

    export function equals(lhs: any, rhs: any) {
        if (lhs === rhs || (lhs !== lhs && rhs !== rhs)) {
            return true;
        }

        if (!lhs || !rhs) {
            return false;
        }

        if (Types.isArray(lhs) && Types.isArray(rhs)) {
            if (lhs.length !== rhs.length) {
                return false;
            }

            for (let i = 0; i < lhs.length; i++) {
                if (!equals(lhs[i], rhs[i])) {
                    return false;
                }
            }

            return true;
        } else if (Types.isObject(lhs) && Types.isObject(rhs)) {
            if (Object.keys(lhs).length !== Object.keys(rhs).length) {
                return false;
            }

            for (let key in lhs) {
                if (lhs.hasOwnProperty(key)) {
                    if (!equals(lhs[key], rhs[key])) {
                        return false;
                    }
                }
            }

            return true;
        }

        return false;
    }

    export function mergeInto(target: object, source: object) {
        if (!Types.isObject(target) || !Types.isObject(source)) {
            return source;
        }

        Object.keys(source).forEach(key => {
            const targetValue = target[key];
            const sourceValue = source[key];

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
}