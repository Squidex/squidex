/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class ApiUrlConfig {
    constructor(public readonly value: string) { }
}

export class DecimalSeparatorConfig {
    constructor(public readonly value: string) { }
}

export class ProductionModeConfig {
    constructor(public readonly isProductionMode: boolean) { }
}

export class UserReportConfig {
    constructor(public readonly siteId: string) { }
}

export class CurrencyConfig {
    constructor(
        public readonly code: string,
        public readonly symbol: string,
        public readonly showAfter = true
    ) {
    }
}