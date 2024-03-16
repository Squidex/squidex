/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable */

interface ReadonlyArray<T> {
    replacedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removed(value?: T): ReadonlyArray<T>;

    defined(): ReadonlyArray<NonNullable<T>>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;

    includes(value: T): boolean;

    toMap(selector: (value: T) => string): { [key: string]: T };
}

interface Array<T> {
    clear(): Array<T>;

    replacedBy(field: keyof T, value: T): ReadonlyArray<T>;

    replaceBy(field: keyof T, value: T): Array<T>;

    removedBy(field: keyof T, value: T): ReadonlyArray<T>;

    removeBy(field: keyof T, value: T): Array<T>;

    removed(value: T): Array<T>;

    remove(value: T): Array<T>;

    defined(): ReadonlyArray<NonNullable<T>>;

    sorted(): ReadonlyArray<T>;

    sortedByString(selector: (value: T) => string): ReadonlyArray<T>;

    sortByString(selector: (value: T) => string): Array<T>;

    includes(value: T): boolean;

    toMap(selector: (value: T) => string): { [key: string]: T };
}

Array.prototype.replaceBy = function<T>(field: keyof T, value: T) {
    const self: T[] = this;

    if (!field || !value) {
        return self;
    }

    for (let i = 0; i < self.length; i++) {
        const item = self[i];

        if (value[field] === item[field]) {
            self[i] = value;
            break;
        }
    }

    return self;
};

Array.prototype.replacedBy = function<T>(field: keyof T, value: T) {
    const self: ReadonlyArray<T> = this;

    if (!field || !value) {
        return self;
    }

    const copy = [...self];

    for (let i = 0; i < self.length; i++) {
        const item = self[i];

        if (value[field] === item[field]) {
            copy[i] = value;
            break;
        }
    }

    return copy;
};

Array.prototype.clear = function<T>() {
    const self: T[] = this;

    self.splice(0, self.length);

    return self;
};

Array.prototype.removeBy = function<T>(field: keyof T, value: T) {
    const self: T[] = this;

    if (!field || !value) {
        return self;
    }

    self.splice(self.findIndex(x => x[field] === value[field]), 1);

    return self;
};

Array.prototype.removed = function<T>(value?: T) {
    const self: ReadonlyArray<T> = this;

    if (!value) {
        return this;
    }

    return self.filter((v: T) => v !== value);
};

Array.prototype.remove = function<T>(value?: T) {
    const self: T[] = this;

    if (!value) {
        return this;
    }

    const index = self.indexOf(value);

    self.splice(index, 1);

    return self;
};

Array.prototype.removedBy = function<T>(field: keyof T, value: T) {
    const self: ReadonlyArray<T> = this;

    if (!field || !value) {
        return self;
    }

    return self.filter((v: T) => v[field] !== value[field]);
};

Array.prototype.sorted = function() {
    const self: any[] = this;

    const copy = [...self];

    copy.sort();

    return copy;
};

Array.prototype.defined = function() {
    const self: any[] = this;

    return self.filter(x => !!x);
}

Array.prototype.includes = function<T>(value: T) {
    const self: any[] = this;

    return self.indexOf(value) >= 0;
}

Array.prototype.sortedByString = function<T>(selector: (value: T) => string) {
    const self: ReadonlyArray<any> = this;

    if (!selector) {
        return self;
    }

    const copy = [...self];

    copy.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return copy;
};

Array.prototype.sortByString = function<T>(selector: (value: T) => string) {
    const self: any[] = this;

    if (!selector) {
        return self;
    }

    self.sort((a, b) => selector(a).localeCompare(selector(b), undefined, { sensitivity: 'base' }));

    return self;
};

Array.prototype.toMap = function<T>(selector: (value: T) => string) {
    const result: { [key: string]: T } = {};

    for (const item of this) {
        result[selector(item)] = item;
    }

    return result;
};
