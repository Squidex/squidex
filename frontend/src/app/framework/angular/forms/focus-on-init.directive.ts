/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, booleanAttribute, Directive, ElementRef, Input } from '@angular/core';
import { Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxFocusOnInit]',
})
export class FocusOnInitDirective implements AfterViewInit {
    @Input({ transform: booleanAttribute })
    public select = false;

    @Input({ alias: 'sqxFocusOnInit', transform: booleanAttribute })
    public enabled = true;

    public scheduler: ((action: (() => void)) => void) = action => {
        setTimeout(action, 200);
    };

    constructor(
        private readonly element: ElementRef<HTMLElement>,
    ) {
    }

    public ngAfterViewInit() {
        if (!this.enabled) {
            return;
        }

        this.scheduler(() => {
            if (Types.isFunction(this.element.nativeElement.focus)) {
                this.element.nativeElement.focus();
            }

            if (this.select) {
                const input = this.element.nativeElement as HTMLInputElement;

                if (Types.isFunction(input.select)) {
                    input.select();
                }
            }
        });
    }
}
