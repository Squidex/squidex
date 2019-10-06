
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

interface ReadonlyArray<T> {
    replaceBy(field: string, value: T): ReadonlyArray<T>;

    removeBy(field: string, value: T): ReadonlyArray<T>;

    removed(value?: T): ReadonlyArray<T>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;
}

interface Array<T> {
    replaceBy(field: string, value: T): Array<T>;

    removeBy(field: string, value: T): Array<T>;

    removed(value?: T): Array<T>;

    sorted(): Array<T>;

    sortedByString(selector: (value: T) => string): Array<T>;
}

Array.prototype.replaceBy = function<T>(field: string, value: T) {
    if (!value) {
        return this;
    }

    return this.map((v: T) => v[field] === value[field] ? value : v);
};

Array.prototype.removeBy = function<T>(field: string, value: T) {
    if (!value) {
        return this;
    }

    return this.filter((v: T) => v[field] !== value[field]);
};

Array.prototype.removed = function<T>(value?: T) {
    if (!value) {
        return this;
    }

    return this.filter((v: T) => v !== value);
};

Array.prototype.sorted = function() {
    const copy = [...this];

    copy.sort();

    return copy;
};

Array.prototype.sortedByString = function<T>(selector: (value: T) => string) {
    const copy = [...this];

    copy.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return copy;
};