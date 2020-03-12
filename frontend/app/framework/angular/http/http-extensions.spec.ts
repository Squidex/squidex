/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { parseError } from './http-extensions';

import { ErrorDto } from './../../utils/error';

describe('ErrorParsing', () => {
    it('should return default when error is javascript exception', () => {
        const response: any = new Error();
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(500, 'Fallback', [], response));
    });

    it('should just forward error dto', () => {
        const response: any = new ErrorDto(500, 'error', []);
        const result = parseError(response, 'Fallback');

        expect(result).toBe(response);
    });

    it('should return default 412 error', () => {
        const response: any = { status: 412 };
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(412, 'Failed to make the update. Another user has made a change. Please reload.', [], response));
    });

    it('should return default 429 error', () => {
        const response: any = { status: 429 };
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(429, 'You have exceeded the maximum limit of API calls.', [], response));
    });

    it('should return error from error object', () => {
        const error = { message: 'My-Message', details: ['My-Detail'] };

        const response: any = { status: 400, error };
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(400, 'My-Message', ['My-Detail'], response));
    });

    it('should return error from error json', () => {
        const error = { message: 'My-Message', details: ['My-Detail'] };

        const response: any = { status: 400, error: JSON.stringify(error) };
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(400, 'My-Message', ['My-Detail'], response));
    });

    it('should return default when object is invalid', () => {
        const error = { text: 'invalid' };

        const response: any = { status: 400, error };
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(500, 'Fallback', [], response));
    });

    it('should return default when json is invalid', () => {
        const error = '{{';

        const response: any = { status: 400, error };
        const result = parseError(response, 'Fallback');

        expect(result).toEqual(new ErrorDto(500, 'Fallback', [], response));
    });
});