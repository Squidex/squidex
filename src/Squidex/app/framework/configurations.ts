/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

@Ng2.Injectable()
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

@Ng2.Injectable()
export class DecimalSeparatorConfig {
    constructor(public readonly value: string) { }
}

@Ng2.Injectable()
export class ProductionModeConfig {
    constructor(public readonly isProductionMode: boolean) { }
}

@Ng2.Injectable()
export class UserReportConfig {
    constructor(public readonly siteId: string) { }
}

@Ng2.Injectable()
export class CurrencyConfig {
    constructor(
        public readonly code: string,
        public readonly symbol: string,
        public readonly showAfter: boolean = true
    ) {
    }
}