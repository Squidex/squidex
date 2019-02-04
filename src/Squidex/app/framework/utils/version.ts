/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class Version {
    constructor(
        public readonly value: string
    ) {
    }

    public eq(other: Version) {
        return other && other.trimmed() === this.trimmed();
    }

    private trimmed(): string {
        if (this.value.startsWith('W/')) {
            return this.value.substr(2);
        } else {
            return this.value;
        }
    }
}

export class Versioned<T> {
    constructor(
        public readonly version: Version,
        public readonly payload: T
    ) {
    }
}