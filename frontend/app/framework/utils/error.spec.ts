/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ErrorDto } from './error';

describe('ErrorDto', () => {
    it('should create simple message when no details are specified.', () => {
        const error = new ErrorDto(500, 'Error Message.');

        expect(error.displayMessage).toBe('Error Message.');
    });

    it('should append dot to message', () => {
        const error = new ErrorDto(500, 'Error Message');

        expect(error.displayMessage).toBe('Error Message.');
    });

    it('should create simple message when detail has one item', () => {
        const error = new ErrorDto(500, 'Error Message.', ['Detail Message.']);

        expect(error.displayMessage).toBe('Error Message.\n\n * Detail Message.\n');
    });

    it('should create simple message with appended dots when detail has one item', () => {
        const error = new ErrorDto(500, 'Error Message', ['Detail Message']);

        expect(error.displayMessage).toBe('Error Message.\n\n * Detail Message.\n');
    });

    it('should create html list when error has more items.', () => {
        const error = new ErrorDto(500, 'Error Message', ['Detail1.', 'Detail2.']);

        expect(error.displayMessage).toBe('Error Message.\n\n * Detail1.\n * Detail2.\n');
    });
});