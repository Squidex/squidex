/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, NgZone, OnDestroy, OnInit, Output } from '@angular/core';
import { ShortcutService, StatefulComponent } from '@app/framework/internal';

@Component({
    selector: 'sqx-shortcut',
    template: '',
})
export class ShortcutComponent extends StatefulComponent implements OnDestroy, OnInit {
    private lastKeys: string;

    @Output()
    public trigger = new EventEmitter();

    @Input()
    public keys: string;

    @Input()
    public disabled?: boolean | null;

    constructor(
        changeDetector: ChangeDetectorRef,
        private readonly shortcutService: ShortcutService,
        private readonly zone: NgZone,
    ) {
        super(changeDetector, {});

        changeDetector.detach();
    }

    public ngOnDestroy() {
        if (this.lastKeys) {
            this.shortcutService.off(this.lastKeys);
        }
    }

    public ngOnInit() {
        this.lastKeys = this.keys;

        if (this.lastKeys) {
            this.shortcutService.on(this.lastKeys, () => {
                if (!this.disabled) {
                    this.zone.run(() => {
                        this.trigger.next(true);
                    });
                }

                return false;
            });
        }
    }
}
