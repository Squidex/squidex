/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input } from '@angular/core';

@Directive({
    selector: '[sqxPopupLink]',
})
export class PopupLinkDirective {
    @Input('sqxPopupLink')
    public url: string;

    @HostListener('click')
    public onClick(): boolean {
        window.open(this.url, '_target', 'location=no,toolbar=no,width=500,height=500,left=100,top=100;');

        return false;
    }
}
