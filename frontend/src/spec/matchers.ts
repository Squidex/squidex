/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export const customMatchers: jasmine.CustomMatcherFactories = {
    toEqualIgnoringProps: (util) => {
        return {
            compare: (actual: any, expected: any, propsToIgnore: string[] = []) => {
                propsToIgnore.push('cachedValues');

                const omit = (obj: any) =>
                    Object.fromEntries(
                        Object.entries(obj).filter(([key]) => !propsToIgnore.includes(key)),
                    );

                const result = {} as jasmine.CustomMatcherResult;
                result.pass = util.equals(
                    omit(actual),
                    omit(expected),
                );

                result.message = result.pass
                    ? 'Expected objects not to be equal (ignoring props), but they are.'
                    : 'Expected objects to be equal (ignoring props), but they are not.';

                return result;
            },
        };
    },
};