/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, NgZone, OnDestroy, OnInit, Output } from '@angular/core';

import { ShortcutService } from './../services/shortcut.service';

@Component({
    selector: 'sqx-shortcut',
    template: ''
})
export class ShortcutComponent implements OnDestroy, OnInit {
    @Input()
    public keys: string;

    @Input()
    public disabled: boolean;

    @Output()
    public trigger = new EventEmitter();

    private lastKeys: string;

    constructor(
        private readonly shortcutService: ShortcutService,
        private readonly zone: NgZone
    ) {
    }

    public ngOnDestroy() {
        if (this.lastKeys) {
            this.shortcutService.off(this.lastKeys);
        }
    }

    public ngOnInit() {
        this.lastKeys = this.keys;

        if (this.lastKeys) {
            this.shortcutService.on(this.lastKeys, e => {
                if (!this.disabled) {
                    this.zone.run(() => {
                        this.trigger.next(e);
                    });
                }

                return false;
            });
        }
    }
}