/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CurrencyConfig, DecimalSeparatorConfig } from '@app/framework/internal';
import { MoneyPipe } from './money.pipe';

describe('MoneyPipe', () => {
    it('should format money values with symbol after number', () => {
        const pipe = new MoneyPipe(new CurrencyConfig('EUR', '€'), new DecimalSeparatorConfig(','));

        const actual = pipe.transform(123.49);
        const expected = '123,<span class="decimal">49</span> €';

        expect(actual).toBe(expected);
    });

    it('should format money values with symbol after number and one decimal', () => {
        const pipe = new MoneyPipe(new CurrencyConfig('EUR', '€'), new DecimalSeparatorConfig(','));

        const actual = pipe.transform(123.4);
        const expected = '123,<span class="decimal">40</span> €';

        expect(actual).toBe(expected);
    });

    it('should format money values with symbol before number', () => {
        const pipe = new MoneyPipe(new CurrencyConfig('EUR', '€', false), new DecimalSeparatorConfig(','));

        const actual = pipe.transform(123.49);
        const expected = '€ 123,<span class="decimal">49</span>';

        expect(actual).toBe(expected);
    });

    it('should format money values with symbol before number and one decimal', () => {
        const pipe = new MoneyPipe(new CurrencyConfig('EUR', '€', false), new DecimalSeparatorConfig(','));

        const actual = pipe.transform(123.4);
        const expected = '€ 123,<span class="decimal">40</span>';

        expect(actual).toBe(expected);
    });
});
