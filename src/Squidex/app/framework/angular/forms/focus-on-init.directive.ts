/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input } from '@angular/core';

import { Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxFocusOnInit]'
})
export class FocusOnInitDirective implements AfterViewInit {
    @Input()
    public select: boolean;

    constructor(
        private readonly element: ElementRef
    ) {
    }

    public ngAfterViewInit() {
        setTimeout(() => {
            if (Types.isFunction(this.element.nativeElement.focus)) {
                this.element.nativeElement.focus();
            }

            if (this.select) {
                if (Types.isFunction(this.element.nativeElement.select)) {
                    this.element.nativeElement.select();
                }
            }
        }, 100);
    }
}