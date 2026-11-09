/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, inject, input } from '@angular/core';
import { copyElementText, DialogService } from '@app/framework/internal';

@Directive({
    selector: '[sqxCopy]',
    host: {
        '(click)': 'onClick()',
    },
})
export class CopyDirective {
    private readonly dialogs = inject(DialogService);

    /** The element whose value or text is copied to the clipboard on click. */
    public readonly inputElement = input<any>(undefined, { alias: 'sqxCopy' });

    protected onClick() {
        const element = this.inputElement();
        if (element) {
            this.copyToClipbord(element);
        }
    }

    private async copyToClipbord(element: HTMLElement) {
        if (await copyElementText(element)) {
            this.dialogs.notifyInfo('i18n:common.clipboardAdded');
        }
    }
}
