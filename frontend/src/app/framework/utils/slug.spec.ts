/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { generateSlug } from './slug';

describe('generateSlug', () => {
    describe('Should replace special characters with separator', () => {
        const cases: Array<[
            string,
            string,
            string
        ]> = [
            ['Hello World', '-', 'hello-world'],
            ['Hello/World', '-', 'hello-world'],
            ['Hello World', '_', 'hello_world'],
            ['Hello/World', '_', 'hello_world'],
            ['Hello World ', '_', 'hello_world'],
            ['Hello World-', '_', 'hello_world'],
            ['Hello/World_', '_', 'hello_world'],
        ];

        cases.forEach(([input, separator, expected]) => {
            it(`slugifies "${input}" with separator "${separator}" to "${expected}"`, () => {
                expect(generateSlug(input, { separator })).toEqual(expected);
            });
        });
    });

    describe('Should replace multi-char diacritics', () => {
        const cases: Array<[
            string,
            string
        ]> = [
            ['ö', 'oe'],
            ['ü', 'ue'],
            ['ä', 'ae'],
        ];

        cases.forEach(([input, expected]) => {
            it(`slugifies "${input}" to "${expected}"`, () => {
                expect(generateSlug(input)).toEqual(expected);
            });
        });
    });

    describe('Should not replace multi-char diacritics', () => {
        const cases: Array<[
            string,
            string
        ]> = [
            ['ö', 'o'],
            ['ü', 'u'],
            ['ä', 'a'],
        ];

        cases.forEach(([input, expected]) => {
            it(`slugifies "${input}" to "${expected}" with singleCharDiacritic=true`, () => {
                expect(generateSlug(input, { singleCharDiacritic: true })).toEqual(expected);
            });
        });
    });

    describe('Should replace single-char diacritics', () => {
        const cases: Array<[
            string,
            string
        ]> = [
            ['Físh', 'fish'],
            ['źish', 'zish'],
            ['żish', 'zish'],
            ['fórm', 'form'],
            ['fòrm', 'form'],
            ['fårt', 'fart'],
        ];

        cases.forEach(([input, expected]) => {
            it(`slugifies "${input}" to "${expected}"`, () => {
                expect(generateSlug(input)).toEqual(expected);
            });
        });
    });

    describe('Should keep characters', () => {
        const cases: Array<[
            string,
            string,
            string
        ]> = [
            ['Hello my&World ', '_', 'hello_my&world'],
            ['Hello my&World-', '_', 'hello_my&world'],
            ['Hello my/World_', '_', 'hello_my/world'],
        ];

        cases.forEach(([input, separator, expected]) => {
            it(`slugifies "${input}" with separator "${separator}" keeping chars`, () => {
                expect(generateSlug(input, { allowed: ['&', '/'], separator })).toEqual(expected);
            });
        });
    });
});
