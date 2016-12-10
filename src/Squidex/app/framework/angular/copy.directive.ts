/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

@Ng2.Directive({
    selector: '[sqxCopy]'
})
export class CopyDirective {
    @Ng2.Input('sqxCopy')
    public inputElement: Ng2.ElementRef;

    @Ng2.HostListener('click')
    public onClick() {
        if (this.inputElement) {
            this.copyToClipbord(this.inputElement.nativeElement);
        }
    }

    private copyToClipbord(element: HTMLInputElement | HTMLTextAreaElement) {
        const  currentFocus: any = document.activeElement;

        const prevSelectionStart = element.selectionStart;
        const prevSelectionEnd = element.selectionEnd;

        element.focus();
        element.setSelectionRange(0, element.value.length);
        
        try {
            document.execCommand('copy');
        } catch (e) {
            console.log('Copy failed');
        }

        if (currentFocus && typeof currentFocus.focus === 'function') {
            currentFocus.focus();
        }
        
        element.setSelectionRange(prevSelectionStart, prevSelectionEnd);
    }
}