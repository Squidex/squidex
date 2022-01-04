/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';
import { ShortcutService } from '@app/framework/internal';

@Directive({
    selector: '[shortcut]',
})
export class ShortcutDirective implements OnDestroy, OnInit {
    private subscription?: Function;

    @Input()
    public shortcut!: string;

    @Input()
    public shortcutAction: 'focus' | 'click' | 'none' = 'click';

    constructor(
        private readonly element: ElementRef,
        private readonly shortcutService: ShortcutService,
    ) {
    }

    public ngOnDestroy() {
        this.subscription?.();
    }

    public ngOnInit() {
        if (this.shortcut && this.shortcutAction !== 'none') {
            this.subscription = this.shortcutService.listen(this.shortcut, () => {
                const target = this.element.nativeElement;

                if (target && !target.disabled) {
                    try {
                        target[this.shortcutAction]();
                    } catch {
                        // NOOP
                    }
                }
            });
        }
    }
}
