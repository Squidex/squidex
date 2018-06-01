/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class Lazy<T> {
    private valueSet = false;
    private valueField: T;

    public get value(): T {
        if (!this.valueSet) {
            this.valueField = this.factory();
            this.valueSet = true;
        }

        return this.valueField;
    }
    constructor(
        private readonly factory: () => T
    ) {
    }
}