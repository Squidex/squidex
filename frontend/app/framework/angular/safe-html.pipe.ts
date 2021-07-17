/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml, SafeResourceUrl, SafeUrl } from '@angular/platform-browser';

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

    public transform(url: string): SafeUrl {
        return this.domSanitizer.bypassSecurityTrustUrl(url);
    }
}

@Pipe({
    name: 'sqxSafeResourceUrl',
    pure: true,
})
export class SafeResourceUrlPipe implements PipeTransform {
    constructor(
        public readonly domSanitizer: DomSanitizer,
    ) {
    }

    public transform(url: string): SafeResourceUrl {
        return this.domSanitizer.bypassSecurityTrustResourceUrl(url);
    }
}
