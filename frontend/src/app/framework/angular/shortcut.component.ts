/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, effect, input, output } from '@angular/core';
import { ShortcutService } from '@app/framework/internal';

@Component({
    selector: 'sqx-shortcut',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortcutComponent {
    /** Emits when the shortcut is pressed while the component is enabled. */
    public readonly trigger = output<boolean>();

    /** The key combination to listen to, for example "ctrl+s". */
    public readonly keys = input('');

    /** Ignores the shortcut while true. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    constructor(shortcutService: ShortcutService) {
        effect(onCleanup => {
            const keys = this.keys();
            if (!keys) {
                return;
            }

            const unsubscribe = shortcutService.listen(keys, () => {
                if (!this.disabled()) {
                    this.trigger.emit(true);
                }

                return false;
            });

            onCleanup(unsubscribe);
        });
    }
}
