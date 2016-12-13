/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';

@Injectable()
export class ApiUrlConfig {
    public readonly value: string;

    constructor(value: string) {
        if (value.indexOf('/', value.length - 1) < 0) {
            value = value + '/';
        }

        this.value = value;
    }

    public buildUrl(path: string) {
        if (path.indexOf('/') === 0) {
            path = path.substr(1);
        }

        return this.value + path;
    }
}

@Injectable()
export class DecimalSeparatorConfig {
    constructor(public readonly value: string) { }
}

@Injectable()
export class ProductionModeConfig {
    constructor(public readonly isProductionMode: boolean) { }
}

@Injectable()
export class UserReportConfig {
    constructor(public readonly siteId: string) { }
}

@Injectable()
export class CurrencyConfig {
    constructor(
        public readonly code: string,
        public readonly symbol: string,
        public readonly showAfter: boolean = true
    ) {
    }
}