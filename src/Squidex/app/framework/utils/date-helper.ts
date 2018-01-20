/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import * as moment from 'moment';

export module DateHelper {
    let momentInstance: any;

    export function setMoment(value: any) {
        momentInstance = value;
    }

    export function locale(code: string) {
        (momentInstance || moment).locale(code);
    }
}