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
    standalone: true,
})
export class FocusOnInitDirective implements AfterViewInit {
    @Input({ transform: booleanAttribute })
    public select = false;

    @Input({ alias: 'sqxFocusOnInit', transform: booleanAttribute })
    public enabled = true;

    constructor(
        private readonly element: ElementRef<HTMLElement>,
    ) {
    }

    public ngAfterViewInit() {
        if (!this.enabled) {
            return;
        }

        setTimeout(() => {
            if (Types.isFunction(this.element.nativeElement.focus)) {
                this.element.nativeElement.focus();
            }

            if (this.select) {
                const input = this.element.nativeElement as HTMLInputElement;

                if (Types.isFunction(input.select)) {
                    input.select();
                }
            }
        }, 100);
    }
}
