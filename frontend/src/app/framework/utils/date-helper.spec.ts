/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateHelper } from './date-helper';

describe('DateHelper', () => {
    it('should use default locale if not configured', () => {
        DateHelper.setlocale(null);

        const locale = DateHelper.getLocale();

        expect(locale).toBe('en');
    });

    it('should use configured locale', () => {
        DateHelper.setlocale('it');

        const locale = DateHelper.getLocale();

        expect(locale).toBe('it');
    });
});
