/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { LocalizerService, Types } from '@app/framework/internal';

@Pipe({
    name: 'sqxTranslate',
    pure: true,
})
export class TranslatePipe implements PipeTransform {
    constructor(
        private readonly localizer: LocalizerService,
    ) {
    }

    public transform(value: any, args?: any): string {
        if (Types.isString(value)) {
            return this.localizer.getOrKey(value, args);
        } else if (value && Types.isFunction(value['translate'])) {
            return value['translate'](this.localizer);
        } else {
            return '';
        }
    }
}
