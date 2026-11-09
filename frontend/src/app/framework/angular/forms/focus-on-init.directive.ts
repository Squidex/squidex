/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, booleanAttribute, Directive, ElementRef, inject, input } from '@angular/core';
import { Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxFocusOnInit]',
})
export class FocusOnInitDirective {
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);

    /** Also selects the text of the element after it has been focused. */
    public readonly select = input(false, { transform: booleanAttribute });

    /** Focuses the element after it has been rendered. */
    public readonly enabled = input(true, { alias: 'sqxFocusOnInit', transform: booleanAttribute });

    public scheduler: ((action: (() => void)) => void) = action => {
        setTimeout(action, 200);
    };

    constructor() {
        afterNextRender(() => {
            if (!this.enabled()) {
                return;
            }

            this.scheduler(() => {
                if (Types.isFunction(this.element.nativeElement.focus)) {
                    this.element.nativeElement.focus();
                }

                if (this.select()) {
                    const input = this.element.nativeElement as HTMLInputElement;

                    if (Types.isFunction(input.select)) {
                        input.select();
                    }
                }
            });
        });
    }
}
