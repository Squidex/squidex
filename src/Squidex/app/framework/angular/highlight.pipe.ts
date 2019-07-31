/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: no-pipe-impure

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxHighlight',
    pure: false
})
export class HighlightPipe implements PipeTransform {
    public transform(text: string, highlight: string): string {
        if (!highlight) {
            return text;
        }

        return text.replace(new RegExp(highlight, 'i'), s => `<b>${s}</b>`);
    }
}