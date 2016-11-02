/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { ShortcutService } from './../services/shortcut.service';

@Ng2.Component({
    selector: 'sqx-shortcut', 
    template: ''
})
export class ShortcutComponent implements Ng2.OnInit, Ng2.OnDestroy {
    @Ng2.Input()
    public keys: string;

    @Ng2.Input()
    public disabled: boolean;

    @Ng2.Output()
    public trigger = new Ng2.EventEmitter();
    
    private lastKeys: string;

    constructor(
        private readonly shortcutService: ShortcutService, 
        private readonly zone: Ng2.NgZone
    ) {
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
    public ngOnDestroy() {
        if (this.lastKeys) {
            this.shortcutService.off(this.lastKeys);
        }
    }
}