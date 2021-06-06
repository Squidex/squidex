/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { enUS, it, nl, zhCN } from 'date-fns/locale';

export module DateHelper {
    let locale: string | null;

    export const FNSLOCALES = {
        en: enUS,
        it,
        nl,
        zh: zhCN,
    };

    export function setlocale(code: string | null) {
        locale = code;
    }

    export function getLocale() {
        return locale || 'en';
    }

    export function getUTCDate(date: Date) {
        return new Date(date.getTime() + date.getTimezoneOffset() * 60 * 1000);
    }

    export function getLocalDate(date: Date) {
        return new Date(date.getTime() - date.getTimezoneOffset() * 60 * 1000);
    }

    export function getFnsLocale(): Locale {
        return DateHelper.FNSLOCALES[DateHelper.getLocale()] || enUS;
    }
}
