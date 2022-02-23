/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class Version {
    public static readonly EMPTY = new Version('');

    constructor(
        public readonly value: string,
    ) {
    }

    public eq(other: Version) {
        return other && other.trimmed() === this.trimmed();
    }

    public toString() {
        return this.value;
    }

    private trimmed(): string {
        if (this.value.startsWith('W/')) {
            return this.value.substring(2);
        } else {
            return this.value;
        }
    }
}

export function versioned<T = any>(version: Version, payload: T = undefined!): Versioned<T> {
    return { version, payload };
}

export type Versioned<T> = Readonly<{ version: Version; payload: T }>;
