/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, inject } from '@angular/core';
import { copyElementText, DialogService } from '@app/framework/internal';

@Directive({
    selector: '[sqxCopyGlobal]',
    host: {
        '(document:click)': 'onClick($event)',
    },
})
export class CopyGlobalDirective {
    private readonly dialogs = inject(DialogService);

    protected onClick(event: MouseEvent) {
        function findElement(target: HTMLElement | null): string | null {
            if (!target) {
                return null;
            }

            const id = target.getAttribute('copy')!;

            if (id) {
                return id;
            }

            return findElement(target.parentElement);
        }

        const toCopy = document.getElementById(findElement(event.target as any)!);

        if (!toCopy) {
            return;
        }

        this.copyToClipbord(toCopy);
    }

    private async copyToClipbord(element: HTMLElement) {
        if (await copyElementText(element)) {
            this.dialogs.notifyInfo('i18n:common.clipboardAdded');
        }
    }
}
