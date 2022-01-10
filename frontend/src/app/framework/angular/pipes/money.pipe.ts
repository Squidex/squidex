/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { CurrencyConfig, DecimalSeparatorConfig } from '@app/framework/internal';

@Pipe({
    name: 'sqxMoney',
    pure: true,
})
export class MoneyPipe implements PipeTransform {
    constructor(
        private readonly currency: CurrencyConfig,
        private readonly separator: DecimalSeparatorConfig,
    ) {
    }

    public transform(value: number): any {
        const money = value.toFixed(2).toString();

        let result = `${money.substr(0, money.length - 3) + this.separator.value}<span class="decimal">${money.substr(money.length - 2, 2)}</span>`;

        if (this.currency.showAfter) {
            result = `${result} ${this.currency.symbol}`;
        } else {
            result = `${this.currency.symbol} ${result}`;
        }

        return result;
    }
}
