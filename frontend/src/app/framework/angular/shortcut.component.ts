/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectorRef, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { ShortcutService } from '@app/framework/internal';

@Component({
    standalone: true,
    selector: 'sqx-shortcut',
    template: '',
})
export class ShortcutComponent implements OnDestroy, OnInit {
    private subscription?: Function;

    @Output()
    public trigger = new EventEmitter();

    @Input()
    public keys = '';

    @Input({ transform: booleanAttribute })
    public disabled?: boolean | null;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly shortcutService: ShortcutService,
    ) {
        changeDetector.detach();
    }

    public ngOnDestroy() {
        this.subscription?.();
    }

    public ngOnInit() {
        if (this.keys) {
            this.subscription = this.shortcutService.listen(this.keys, () => {
                if (!this.disabled) {
                    this.trigger.next(true);
                }

                return false;
            });
        }
    }
}
