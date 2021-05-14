/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input, Renderer2 } from '@angular/core';
import { DialogService, Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxCopy]',
})
export class CopyDirective {
    @Input('sqxCopy')
    public inputElement: any;

    constructor(
        private readonly dialogs: DialogService,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('click')
    public onClick() {
        if (this.inputElement) {
            this.copyToClipbord(this.inputElement);
        }
    }

    private copyToClipbord(element: HTMLElement) {
        if (Types.is(element, HTMLInputElement) || Types.is(element, HTMLTextAreaElement)) {
            const currentFocus: any = document.activeElement;

            const prevSelectionStart = element.selectionStart;
            const prevSelectionEnd = element.selectionEnd;

            element.focus();
            element.setSelectionRange(0, element.value.length);

            this.copy();

            element.setSelectionRange(prevSelectionStart!, prevSelectionEnd!);

            if (currentFocus && Types.isFunction(currentFocus.focus)) {
                currentFocus.focus();
            }
        } else {
            const input = this.renderer.createElement('textarea');

            this.renderer.setStyle(input, 'position', 'absolute');
            this.renderer.setStyle(input, 'right', '-1000px');
            this.renderer.appendChild(document.body, input);

            input.value = element.innerText;
            input.select();

            this.copy();

            this.renderer.removeChild(document.body, input);
        }
    }

    private copy() {
        try {
            document.execCommand('copy');

            this.dialogs.notifyInfo('i18n:common.clipboardAdded');

            return true;
        } catch (e) {
            return false;
        }
    }
}
