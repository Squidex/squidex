/* eslint-disable @typescript-eslint/object-curly-spacing */
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import './array-extensions';
import { FloatConverter, getTagValues, IntConverter, StringConverter, TagValue } from './tag-values';

describe('TagValue', () => {
    it('should create with value and search string', () => {
        const value = new TagValue(12, 'Tag-Name', 'Tag-Value');

        expect(value.id).toEqual(12);
        expect(value.name).toEqual('Tag-Name');
        expect(value.value).toEqual('Tag-Value');
        expect(value.lowerCaseName).toEqual('tag-name');
        expect(value.toString()).toEqual('Tag-Name');
    });

    it('should get sorted tags if input is array of string', () => {
        const input = [
            '2',
            '1',
            '0',
        ];

        const result = getTagValues(input);

        expect(result).toEqual([
            new TagValue('0', '0', '0'),
            new TagValue('1', '1', '1'),
            new TagValue('2', '2', '2'),
        ]);
    });

    it('should get sorted tags if input is array of tags', () => {
        const input = [
            new TagValue(2, '2', 2),
            new TagValue(1, '1', 1),
            new TagValue(0, '0', 0),
        ];

        const result = getTagValues(input);

        expect(result).toEqual([
            new TagValue(0, '0', 0),
            new TagValue(1, '1', 1),
            new TagValue(2, '2', 2),
        ]);
    });

    [null, undefined, []].forEach(input => {
        it(`should get tags as empty array if input is <${input}>`, () => {
            const result = getTagValues(input);

            expect(result).toEqual([]);
        });
    });

    describe('IntConverter', () => {
        [
            { input: '7.5', result: new TagValue(7, '7.5', 7) },
            { input: '7', result: new TagValue(7, '7', 7) },
            { input: '0', result: new TagValue(0, '0', 0) },
        ].forEach(x => {
            it(`should return tag value if input is <${x.input}>`, () => {
                const result = IntConverter.INSTANCE.convertInput(x.input);

                expect(result).toEqual(x.result);
            });
        });

        [
            { input: undefined },
            { input: null },
            { input: 'text' },
            { input: '' },
        ].forEach(x => {
            it(`should not return tag value if input is <${x.input}>`, () => {
                const result = IntConverter.INSTANCE.convertInput(x.input!);

                expect(result).toBeNull();
            });
        });

        [
            { input: 7, result: new TagValue(7, '7', 7) },
            { input: 0, result: new TagValue(0, '0', 0) },
        ].forEach(x => {
            it(`should return tag value if value is <${x.input}>`, () => {
                const result = IntConverter.INSTANCE.convertValue(x.input);

                expect(result).toEqual(x.result);
            });
        });

        [
            { input: undefined },
            { input: null },
            { input: 'text' },
            { input: '' },
        ].forEach(x => {
            it(`should not return tag value if value is <${x.input}>`, () => {
                const result = IntConverter.INSTANCE.convertValue(x.input!);

                expect(result).toBeNull();
            });
        });
    });

    describe('FloatConverter', () => {
        [
            { input: '7.5', result: new TagValue(7.5, '7.5', 7.5) },
            { input: '0.0', result: new TagValue(0, '0.0', 0) },
            { input: '0', result: new TagValue(0, '0', 0) },
        ].forEach(x => {
            it(`should return tag value if input is <${x.input}>`, () => {
                const result = FloatConverter.INSTANCE.convertInput(x.input);

                expect(result).toEqual(x.result);
            });
        });

        [
            { input: undefined },
            { input: null },
            { input: 'text' },
            { input: '' },
        ].forEach(x => {
            it(`should not return tag value if input is <${x.input}>`, () => {
                const result = FloatConverter.INSTANCE.convertInput(x.input!);

                expect(result).toBeNull();
            });
        });

        [
            { input: 7.5, result: new TagValue(7.5, '7.5', 7.5) },
            { input: 7, result: new TagValue(7, '7', 7) },
            { input: 0, result: new TagValue(0, '0', 0) },
        ].forEach(x => {
            it(`should return tag value if value is <${x.input}>`, () => {
                const result = FloatConverter.INSTANCE.convertValue(x.input);

                expect(result).toEqual(x.result);
            });
        });

        [
            { input: undefined },
            { input: null },
            { input: 'text' },
            { input: '' },
        ].forEach(x => {
            it(`should not return tag value if value is <${x.input}>`, () => {
                const result = FloatConverter.INSTANCE.convertValue(x.input!);

                expect(result).toBeNull();
            });
        });
    });

    describe('StringConverter', () => {
        [
            { input: 'text', result: new TagValue('text', 'text', 'text') },
            { input: 'text ', result: new TagValue('text', 'text', 'text') },
        ].forEach(x => {
            it(`should return tag value if input is <${x.input}>`, () => {
                const result = StringConverter.INSTANCE.convertInput(x.input);

                expect(result).toEqual(x.result);
            });
        });

        [
            { input: undefined },
            { input: null },
            { input: '' },
        ].forEach(x => {
            it(`should not return tag value if input is <${x.input}>`, () => {
                const result = FloatConverter.INSTANCE.convertInput(x.input!);

                expect(result).toBeNull();
            });
        });

        [
            { input: 'text', result: new TagValue('text', 'text', 'text') },
            { input: 'text ', result: new TagValue('text', 'text', 'text') },
        ].forEach(x => {
            it(`should return tag value if value is <${x.input}>`, () => {
                const result = StringConverter.INSTANCE.convertValue(x.input);

                expect(result).toEqual(x.result);
            });
        });

        [
            { input: undefined },
            { input: null },
            { input: ''},
        ].forEach(x => {
            it(`should not return tag value if value is <${x.input}>`, () => {
                const result = FloatConverter.INSTANCE.convertValue(x.input!);

                expect(result).toBeNull();
            });
        });
    });
});
