/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, Output } from '@angular/core';
import ResizeObserver from 'resize-observer-polyfill';

const entriesMap = new WeakMap();

const observer = new ResizeObserver(entries => {
    for (const entry of entries) {
        if (entriesMap.has(entry.target)) {
            const component = entriesMap.get(entry.target);

            component.onResized(entry);
        }
    }
});

@Directive({
    selector: '[sqxResized], [sqxResizeCondition]'
})
export class ResizedDirective implements OnDestroy, OnChanges {
    private condition: ((rect: ClientRect) => boolean) | undefined;
    private conditionValue = false;

    @Input('sqxResizeMinWidth')
    public minWidth?: number;

    @Input('sqxResizeMaxWidth')
    public maxWidth?: number;

    @Output('sqxResizeCondition')
    public resizeCondition = new EventEmitter<boolean>();

    @Output('sqxResized')
    public resize = new EventEmitter<ClientRect>();

    constructor(
        private readonly element: ElementRef
    ) {
        entriesMap.set(element.nativeElement, this);

        observer.observe(element.nativeElement);
    }

    public ngOnChanges() {
        const minWidth = parseInt(<any>this.minWidth, 10);
        const maxWidth = parseInt(<any>this.maxWidth, 10);

        if (minWidth > 0 &&  maxWidth > 0) {
            this.condition = rect => rect.width < minWidth! || rect.width > maxWidth!;
        } else if ( maxWidth > 0) {
            this.condition = rect => rect.width > maxWidth!;
        } else if (minWidth > 0) {
            this.condition = rect => rect.width < minWidth!;
        } else {
            this.condition = undefined;
        }
    }

    public ngOnDestroy() {
        observer.unobserve(this.element.nativeElement);
    }

    public onResized(entry: ResizeObserverEntry) {
        if (this.condition) {
            const value = this.condition(entry.contentRect);

            if (this.conditionValue !== value) {
                this.resizeCondition.emit(value);

                this.conditionValue = value;
            }
        } else {
            this.resize.emit(entry.contentRect);
        }
    }
}