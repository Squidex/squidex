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
                        Object.entries(obj).filter(([key, value]) => value !== undefined && !propsToIgnore.includes(key)),
                    );

                const diffBuilder = new (jasmine as any)['DiffBuilder']({ prettyPrinter: util.pp });

                const result = {} as jasmine.CustomMatcherResult;
                result.pass = (util as any)['equals'](
                    omit(actual),
                    omit(expected),
                    diffBuilder,
                );

                result.message = diffBuilder.getMessage();

                return result;
            },
        };
    },
};