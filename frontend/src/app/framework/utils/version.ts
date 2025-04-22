/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

export class VersionTag {
    public static readonly EMPTY = new VersionTag('');

    constructor(
        public readonly value: string,
    ) {
    }

    public eq(other: VersionTag) {
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

export type Version = number;
export type VersionOrTag = VersionTag | Version;

export function getActualVersion(source: VersionOrTag | undefined): string | number | undefined {
    if (Types.is(source, VersionTag)) {
        return source.value;
    } else {
        return source;
    }
}

export function versioned<T = any>(version: VersionTag, payload: T = undefined!): Versioned<T> {
    return { version, payload };
}

export type Versioned<T> = Readonly<{ version: VersionTag; payload: T }>;
