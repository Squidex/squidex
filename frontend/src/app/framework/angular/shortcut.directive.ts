/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/directive-selector */

import { Directive, effect, ElementRef, inject, input } from '@angular/core';
import { ShortcutService } from '@app/framework/internal';

@Directive({
    selector: '[shortcut]',
})
export class ShortcutDirective {
    private readonly element = inject(ElementRef);
    private readonly shortcutService = inject(ShortcutService);

    /** The key combination to listen to, for example "ctrl+s". */
    public readonly shortcut = input<string | null | undefined>();

    /** What happens with the element when the shortcut is pressed. */
    public readonly shortcutAction = input<'focus' | 'click' | 'none'>('click');

    constructor() {
        effect(onCleanup => {
            const shortcut = this.shortcut();
            if (!shortcut) {
                return;
            }

            const shortcutAction = this.shortcutAction();
            if (shortcutAction === 'none') {
                return;
            }

            const unsubscribe = this.shortcutService.listen(shortcut, () => {
                const target = this.element.nativeElement;

                if (target && !target.disabled) {
                    try {
                        target[shortcutAction]();
                    } catch {
                        // NOOP
                    }
                }
            });

            onCleanup(unsubscribe);
        });
    }
}
