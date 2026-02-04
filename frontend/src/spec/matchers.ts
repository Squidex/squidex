/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect } from 'vitest';

interface CustomMatchers<R = unknown> {
    toEqualIgnoringProps(expected: any, propsToIgnore?: string[]): R;
}

declare module 'vitest' {
    interface Assertion<T = any> extends CustomMatchers<T> {}
    interface AsymmetricMatchersContaining extends CustomMatchers {}
}

expect.extend({
    toEqualIgnoringProps(
        actual: any,
        expected: any,
        propsToIgnore: string[] = [],
    ) {
        propsToIgnore.push('cachedValues');

        const omit = (obj: any) =>
            Object.fromEntries(
                Object.entries(obj).filter(
                    ([key, value]) =>
                        value !== undefined && !propsToIgnore.includes(key),
                ),
            );

        const actualFiltered = omit(actual);
        const expectedFiltered = omit(expected);

        const { isNot } = this;
        const pass = this.equals(actualFiltered, expectedFiltered);

        return {
            pass,
            message: () => {
                const hint = this.utils.matcherHint(
                    'toEqualIgnoringProps',
                    undefined,
                    undefined,
                    { isNot },
                );

                const diff = pass
                    ? ''
                    : this.utils.diff(expectedFiltered, actualFiltered);

                return `${hint}\n\n${diff}`;
            },
        };
    },
});
