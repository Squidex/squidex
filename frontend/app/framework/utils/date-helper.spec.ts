﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import moment from 'moment';

import { DateHelper } from './date-helper';

describe('DateHelper', () => {
    it('should call config method of moment object', () => {
        let called: string;

        DateHelper.setMoment({
            locale: (l: string) => { called = l; }
        });

        DateHelper.locale('en');

        expect(called!).toBe('en');
    });

    it('should use global moment if not configured', () => {
        DateHelper.setMoment(null);
        DateHelper.locale('en');

        expect(moment.locale()).toBe('en');
    });
});
