/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

interface ReadonlyArray<T> {
    replacedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removed(value?: T): ReadonlyArray<T>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;
}

interface Array<T> {
    replacedBy(field: keyof T, value: T): ReadonlyArray<T>;

    replaceBy(field: keyof T, value: T): Array<T>;

    removedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removeBy(field: keyof T, value: T): Array<T>;

    removed(value: T): ReadonlyArray<T>;

    remove(value: T): Array<T>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;

    sortByString(selector: (value: T) => string): Array<T>;
}

Array.prototype.replaceBy = function<T>(field: keyof T, value: T) {
    const self: T[] = this;

    if (!field) {
        return self;
    }

    const index = self.findIndex(v => v[field] === value[field]);

    if (index >= 0) {
        self[index] = value;
    }

    return self;
};

Array.prototype.replacedBy = function<T>(field: keyof T, value: T) {
    const self: ReadonlyArray<T> = this;

    if (!field) {
        return self;
    }

    return self.map((v: T) => v[field] === value[field] ? value : v);
};

Array.prototype.removedBy = function<T>(field: keyof T, value: T) {
    const self: ReadonlyArray<T> = this;

    if (!field) {
        return self;
    }

    return self.filter((v: T) => v[field] !== value[field]);
};

Array.prototype.removeBy = function<T>(field: keyof T, value: T) {
    const self: T[] = this;

    if (!field) {
        return self;
    }

    return self.splice(self.findIndex(x => x[field] === value[field]), 1);
};

Array.prototype.removed = function<T>(value?: T) {
    if (!value) {
        return this;
    }

    const self: ReadonlyArray<T> = [];

    return self.filter((v: T) => v !== value);
};

Array.prototype.remove = function<T>(value?: T) {
    if (!value) {
        return this;
    }

    const self: T[] = [];

    return self.splice(this.indexOf(value), 1);
};

Array.prototype.sorted = function() {
    const self: any[] = this;

    const copy = [...self];

    copy.sort();

    return copy;
};

Array.prototype.sortedByString = function<T>(selector: (value: T) => string) {
    const self: ReadonlyArray<any> = [];

    if (!selector) {
        return self;
    }

    const copy = [...this];

    copy.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return copy;
};

Array.prototype.sortByString = function<T>(selector: (value: T) => string) {
    if (!selector) {
        return this;
    }

    const copy = [...this];

    copy.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return copy;
};