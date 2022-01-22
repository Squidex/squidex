/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export function isArray(value: any): value is any[] {
    return Array.isArray(value);
}

export function isString(value: any): value is string {
    return typeof value === 'string' || value instanceof String;
}

export function isUndefined(value: any): value is undefined {
    return typeof value === 'undefined';
}

export function isBoolean(value: any): value is boolean {
    return typeof value === 'boolean';
}

export function isFunction(value: any): value is Function {
    return typeof value === 'function';
}

export function isNumber(value: any): value is number {
    return typeof value === 'number' && Number.isFinite(value);
}

export function isObject(value: any): value is Object {
    return value && typeof value === 'object' && value.constructor === Object;
}

export function getBaseUrl() {
    let url = (document.currentScript as any)?.['src'] as string;

    if (!isString(url)) {
        return null;
    }

    url = url.trim();

    let indexOfHash = url.indexOf('/', 'https://'.length);

    if (indexOfHash > 0) {
        url = url.substring(0, indexOfHash);
    }

    return url;
}