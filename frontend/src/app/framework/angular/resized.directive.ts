/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { computed, Directive, effect, ElementRef, inject, input, NgZone, numberAttribute, output } from '@angular/core';
import { ResizeListener, ResizeService } from '@app/framework/internal';

@Directive({
    selector: '[sqxResized], [sqxResizeCondition]',
})
export class ResizedDirective implements ResizeListener {
    private readonly element = inject(ElementRef);
    private readonly resizeService = inject(ResizeService);
    private readonly zone = inject(NgZone);
    private readonly condition = computed(() => {
        const minWidth = this.minWidth();
        const maxWidth = this.maxWidth();

        if (minWidth > 0 && maxWidth > 0) {
            return (rect: DOMRect) => rect.width < minWidth || rect.width > maxWidth;
        } else if (maxWidth > 0) {
            return (rect: DOMRect) => rect.width > maxWidth;
        } else if (minWidth > 0) {
            return (rect: DOMRect) => rect.width < minWidth;
        } else {
            return undefined;
        }
    });
    private conditionValue = false;

    /** The condition is true when the element is smaller than this width. */
    public readonly minWidth = input(0, { alias: 'sqxResizeMinWidth', transform: numberAttribute });

    /** The condition is true when the element is wider than this width. */
    public readonly maxWidth = input(0, { alias: 'sqxResizeMaxWidth', transform: numberAttribute });

    /** Emits when the min or max width condition changes. */
    public readonly resizeCondition = output<boolean>({ alias: 'sqxResizeCondition' });

    /** Emits the new size when the element is resized and no width condition is defined. */
    public readonly resizeChange = output<DOMRect>({ alias: 'sqxResized' });

    constructor() {
        effect(onCleanup => {
            const unlisten = this.resizeService.listen(this.element.nativeElement, this);

            onCleanup(unlisten);
        });
    }

    public onResize(rect: DOMRect) {
        const condition = this.condition();
        if (!condition) {
            this.zone.run(() => {
                this.resizeChange.emit(rect);
            });
            return;
        }

        const value = condition(rect);
        if (this.conditionValue !== value) {
            this.zone.run(() => {
                this.resizeCondition.emit(value);
            });

            this.conditionValue = value;
        }
    }
}
