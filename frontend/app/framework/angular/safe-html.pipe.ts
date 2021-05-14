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
    pure: true,
})
export class SafeHtmlPipe implements PipeTransform {
    constructor(
        public readonly domSanitizer: DomSanitizer,
    ) {
    }

    public transform(html: string): SafeHtml {
        return this.domSanitizer.bypassSecurityTrustHtml(html);
    }
}

@Pipe({
    name: 'sqxSafeUrl',
    pure: true,
})
export class SafeUrlPipe implements PipeTransform {
    constructor(
        public readonly domSanitizer: DomSanitizer,
    ) {
    }

    public transform(url: string): SafeHtml {
        return this.domSanitizer.bypassSecurityTrustUrl(url);
    }
}
