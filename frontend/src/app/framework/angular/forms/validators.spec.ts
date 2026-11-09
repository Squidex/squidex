/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from '@app/framework/internal';
import { ValidatorsEx } from './validators';

describe('ValidatorsEx', () => {
    describe('between', () => {
        it('should return null validator if no min value or max value', () => {
            const validator = ValidatorsEx.between(undefined, undefined);

            expect(validator).toBe(ValidatorsEx.nullValidator);
        });

        it('should return null if value is equal to min and max', () => {
            const input = 3;

            const error = <any>ValidatorsEx.between(3, 3)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is valid', () => {
            const input = 4;

            const error = <any>ValidatorsEx.between(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is null', () => {
            const input = null;

            const error = <any>ValidatorsEx.between(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is undefined', () => {
            const input = undefined;

            const error = <any>ValidatorsEx.between(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return error if less than min', () => {
            const input = 0;

            const error = <any>ValidatorsEx.between(1, undefined)(input);

            expect(error.kind).toBe('min');
        });

        it('should return error if greater than max', () => {
            const input = 6;

            const error = <any>ValidatorsEx.between(undefined, 5)(input);

            expect(error.kind).toBe('max');
        });

        it('should return error if not in range', () => {
            const input = 1;

            const error = <any>ValidatorsEx.between(2, 4)(input);

            expect(error.kind).toBe('between');
        });

        it('should return error if not equal to min and max', () => {
            const input = 2;

            const error = <any>ValidatorsEx.between(3, 3)(input);

            expect(error.kind).toBe('exactly');
        });
    });

    describe('betweenLength', () => {
        it('should return null validator if no min value or max value', () => {
            const validator = ValidatorsEx.betweenLength(undefined, undefined);

            expect(validator).toBe(ValidatorsEx.nullValidator);
        });

        it('should return null if value is equal to min and max', () => {
            const input = 'xxx';

            const error = <any>ValidatorsEx.betweenLength(3, 3)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is valid', () => {
            const input = 'xxxx';

            const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is null', () => {
            const input = null;

            const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is undefined', () => {
            const input = undefined;

            const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return error if less than min', () => {
            const input = 'x';

            const error = <any>ValidatorsEx.betweenLength(2, undefined)(input);

            expect(error.kind).toBe('minlength');
        });

        it('should return error if greater than max', () => {
            const input = 'xxxxxx';

            const error = <any>ValidatorsEx.betweenLength(undefined, 5)(input);

            expect(error.kind).toBe('maxlength');
        });

        it('should return error if not in range', () => {
            const input = 'x';

            const error = <any>ValidatorsEx.betweenLength(2, 4)(input);

            expect(error.kind).toBe('betweenlength');
        });

        it('should return error if not equal to min and max', () => {
            const input = 'xx';

            const error = <any>ValidatorsEx.betweenLength(3, 3)(input);

            expect(error.kind).toBe('exactlylength');
        });
    });

    describe('validDateTime', () => {
        it('should return null validator if valid is not defined', () => {
            const input = null;

            const error = <any>ValidatorsEx.validDateTime()(input);

            expect(error).toBeNull();
        });

        it('should return null if date time is valid', () => {
            const input = DateTime.now().toISOString();

            const error = ValidatorsEx.validDateTime()(input);

            expect(error).toBeNull();
        });

        it('should return error if value is invalid date', () => {
            const input = 'invalid';

            const error = <any>ValidatorsEx.validDateTime()(input);

            expect(error.kind).toBe('validdatetime');
        });
    });

    describe('validValues', () => {
        it('should return null validator if values not defined', () => {
            const validator = ValidatorsEx.validValues(null!);

            expect(validator).toBe(ValidatorsEx.nullValidator);
        });

        it('should return null if value is in allowed values', () => {
            const input = 10;

            const error = ValidatorsEx.validValues([10, 20, 30])(input);

            expect(error).toBeNull();
        });

        it('should return error if value is not in allowed values', () => {
            const input = 50;

            const error = <any>ValidatorsEx.validValues([10, 20, 30])(input);

            expect(error.kind).toBe('validvalues');
        });
    });

    describe('validArrayValues', () => {
        it('should return null validator if values not defined', () => {
            const validator = ValidatorsEx.validArrayValues(null!);

            expect(validator).toBe(ValidatorsEx.nullValidator);
        });

        it('should return null if value is in allowed values', () => {
            const input = [10, 20];

            const error = ValidatorsEx.validArrayValues([10, 20, 30])(input);

            expect(error).toBeNull();
        });

        it('should return error if value is not in allowed values', () => {
            const input = [50];

            const error = <any>ValidatorsEx.validArrayValues([10, 20, 30])(input);

            expect(error.kind).toBe('validarrayvalues');
        });
    });

    describe('pattern', () => {
        it('should return null validator if pattern not defined', () => {
            const validator = ValidatorsEx.pattern(undefined!, undefined);

            expect(validator).toBe(ValidatorsEx.nullValidator);
        });

        it('should return null if value is valid pattern', () => {
            const input = '1234';

            const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is null string', () => {
            const input = null;

            const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is empty string', () => {
            const input = '';

            const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

            expect(error).toBeNull();
        });

        it('should return error with message if value does not match pattern string', () => {
            const input = 'abc';

            const error = <any>ValidatorsEx.pattern('[0-9]{1,4}', 'My-Message')(input);
            const expected: any = {
                kind: 'patternmessage', requiredPattern: '^[0-9]{1,4}$', actualValue: 'abc', message: 'My-Message',
            };

            expect(error).toEqual(expected);
        });

        it('should return error with message if value does not match pattern', () => {
            const input = 'abc';

            const error = <any>ValidatorsEx.pattern(/^[0-9]{1,4}$/, 'My-Message')(input);
            const expected: any = {
                kind: 'patternmessage', requiredPattern: '/^[0-9]{1,4}$/', actualValue: 'abc', message: 'My-Message',
            };

            expect(error).toEqual(expected);
        });

        it('should return error without message if value does not match pattern string', () => {
            const input = 'abc';

            const error = <any>ValidatorsEx.pattern('[0-9]{1,4}')(input);
            const expected: any = {
                kind: 'pattern', requiredPattern: '^[0-9]{1,4}$', actualValue: 'abc',
            };

            expect(error).toEqual(expected);
        });

        it('should return error without message if value does not match pattern', () => {
            const input = 'abc';

            const error = <any>ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);
            const expected: any = {
                kind: 'pattern', requiredPattern: '/^[0-9]{1,4}$/', actualValue: 'abc',
            };

            expect(error).toEqual(expected);
        });
    });

    describe('uniqueStrings', () => {
        it('should return null if value is null', () => {
            const input = null;

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toBeNull();
        });

        it('should return null if value is not a string array', () => {
            const input = [1, 2, 3];

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toBeNull();
        });

        it('should return null if values are unique', () => {
            const input = ['1', '2', '3'];

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toBeNull();
        });

        it('should return error if values are not unique', () => {
            const input = ['1', '2', '2', '3'];

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toEqual({ kind: 'uniquestrings' });
        });
    });

    describe('uniqueObjectValues', () => {
        it('should return null if value is null', () => {
            const input = null;

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if value is not an object array', () => {
            const input = [1, 2, 3];

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if values array has one item', () => {
            const input = [{}];

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if values array has no duplicate', () => {
            const input = [{ myString: '1' }, { myString: '2' }];

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if values array has unchecked duplicate', () => {
            const input = [{ other: '1' }, { other: '1' }];

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return error if values array has duplicate', () => {
            const input = [{ myString: '1' }, { myString: '1' }];

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toEqual({ kind: 'uniqueobjectvalues', fields: 'myString' });
        });

        it('should return error if values array has multiple duplicates', () => {
            const input = [{ myString: '1', myNumber: 2 }, { myString: '1', myNumber: 2 }];

            const error = ValidatorsEx.uniqueObjectValues(['myString', 'myNumber'])(input);

            expect(error).toEqual({ kind: 'uniqueobjectvalues', fields: 'myString, myNumber' });
        });
    });
});
