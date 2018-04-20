/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module Types {
    export function isString(value: any): boolean {
        return typeof value === 'string' || value instanceof String;
    }

    export function isNumber(value: any): boolean {
        return typeof value === 'number' && isFinite(value);
    }

    export function isArray(value: any): boolean {
        return Array.isArray(value);
    }

    export function isFunction(value: any): boolean {
        return typeof value === 'function';
    }

    export function isObject(value: any): boolean {
        return value && typeof value === 'object' && value.constructor === Object;
    }

    export function isBoolean(value: any): boolean {
        return typeof value === 'boolean';
    }

    export function isNull(value: any): boolean {
        return value === null;
    }

    export function isUndefined(value: any): boolean {
        return typeof value === 'undefined';
    }

    export function isRegExp(value: any): boolean {
        return value && typeof value === 'object' && value.constructor === RegExp;
    }

    export function isDate(value: any): boolean {
        return value instanceof Date;
    }

    export function isArrayOfNumber(value: any): boolean {
        return isArrayOf(value, v => isNumber(v));
    }

    export function isArrayOfString(value: any): boolean {
        return isArrayOf(value, v => isString(v));
    }

    export function isArrayOf(value: any, validator: (v: any) => boolean): boolean {
        if (!Array.isArray(value)) {
            return false;
        }

        for (let v of value) {
            if (!validator(v)) {
                return false;
            }
        }

        return true;
    }

    export function isEquals<T>(lhs: T[], rhs: T[]) {
        if (!lhs && !rhs) {
            return true;
        }

        if (lhs.length !== rhs.length) {
            return false;
        }

        for (let i = 0; i < lhs.length; i++) {
            if (rhs[i] !== lhs[i]) {
                return false;
            }
        }

        return true;
    }
}