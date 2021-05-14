/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { Types } from '@app/framework/internal';

@Pipe({
    name: 'sqxHighlight',
    pure: false,
})
export class HighlightPipe implements PipeTransform {
    public transform(text: string, highlight: string | RegExp | undefined | null): string {
        if (!highlight) {
            return text;
        }

        if (Types.isString(highlight)) {
            highlight = new RegExp(highlight, 'i');
        }

        return text.replace(highlight, s => `<b>${s}</b>`);
    }
}
