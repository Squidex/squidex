/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

export class TagValue<T = any> {
    public readonly lowerCaseName: string;

    constructor(
        public readonly id: any,
        public readonly name: string,
        public readonly value: T,
    ) {
        this.lowerCaseName = name.toLowerCase();
    }

    public toString() {
        return this.name;
    }
}

export interface TagConverter {
    convertInput(input: string): TagValue | null;

    convertValue(value: any): TagValue | null;
}

export class IntConverter implements TagConverter {
    private static ZERO = new TagValue(0, '0', 0);

    public static readonly INSTANCE: TagConverter = new IntConverter();

    private constructor() {
        /* NOOP */
    }

    public convertInput(input: string) {
        if (input === '0') {
            return IntConverter.ZERO;
        }

        const parsed = parseInt(input, 10);

        if (Types.isNumber(parsed)) {
            return new TagValue(parsed, input, parsed);
        }

        return null;
    }

    public convertValue(value: any) {
        if (Types.isNumber(value)) {
            return new TagValue(value, `${value}`, value);
        }

        return null;
    }
}

export class FloatConverter implements TagConverter {
    private static ZERO = new TagValue(0, '0', 0);

    public static readonly INSTANCE: TagConverter = new FloatConverter();

    private constructor() {
        /* NOOP */
    }

    public convertInput(input: string) {
        if (input === '0') {
            return FloatConverter.ZERO;
        }

        const parsed = parseFloat(input);

        if (Types.isNumber(parsed)) {
            return new TagValue(parsed, input, parsed);
        }

        return null;
    }

    public convertValue(value: any) {
        if (Types.isNumber(value)) {
            return new TagValue(value, `${value}`, value);
        }

        return null;
    }
}

export class StringConverter implements TagConverter {
    public static readonly INSTANCE: TagConverter = new StringConverter();

    private constructor() {
        /* NOOP */
    }

    public convertInput(input: string) {
        if (input) {
            const trimmed = input.trim();

            if (trimmed.length > 0) {
                return new TagValue(trimmed, trimmed, trimmed);
            }
        }

        return null;
    }

    public convertValue(value: any) {
        if (Types.isString(value)) {
            const trimmed = value.trim();

            return new TagValue(trimmed, trimmed, trimmed);
        }

        return null;
    }
}

export function getTagValues(values: ReadonlyArray<string | TagValue> | undefined | null) {
    if (!Types.isArray(values)) {
        return [];
    }

    const result: TagValue[] = [];

    for (const value of values) {
        if (Types.isString(value)) {
            result.push(new TagValue(value, value, value));
        } else {
            result.push(value);
        }
    }

    return result.sortByString(x => x.lowerCaseName);
}
