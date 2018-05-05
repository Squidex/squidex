/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input } from '@angular/core';

import {
    DialogService,
    Notification,
    Types
} from '@app/framework/internal';

@Directive({
    selector: '[sqxCopy]'
})
export class CopyDirective {
    @Input('sqxCopy')
    public inputElement: any;

    constructor(
        private readonly dialogs: DialogService
    ) {
    }

    @HostListener('click')
    public onClick() {
        if (this.inputElement) {
            this.copyToClipbord(this.inputElement);
        }
    }

    private copyToClipbord(element: HTMLInputElement | HTMLTextAreaElement) {
        const  currentFocus: any = document.activeElement;

        const prevSelectionStart = element.selectionStart;
        const prevSelectionEnd = element.selectionEnd;

        element.focus();

        if (Types.is(element, HTMLInputElement)) {
            element.setSelectionRange(0, element.value.length);
        }

        try {
            document.execCommand('copy');

            this.dialogs.notify(Notification.info('Value has been added to your clipboard.'));
        } catch (e) {
            console.log('Copy failed');
        }

        if (currentFocus && Types.isFunction(currentFocus.focus)) {
            currentFocus.focus();
        }

        if (Types.is(element, HTMLInputElement)) {
            element.setSelectionRange(prevSelectionStart!, prevSelectionEnd!);
        }
    }
}