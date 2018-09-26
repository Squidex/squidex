/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
    name: 'sqxSafeHtml',
    pure: true
})
export class SafeHtmlPipe implements PipeTransform {
    constructor(
        public readonly domSanitizer: DomSanitizer
    ) {
    }

    public transform(html: string): SafeHtml {
        return this.domSanitizer.bypassSecurityTrustHtml(html);
    }
}