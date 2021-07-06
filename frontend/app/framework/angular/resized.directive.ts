/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, Input, NgZone, OnChanges, OnDestroy, Output } from '@angular/core';
import { ResizeListener, ResizeService, ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxResized], [sqxResizeCondition]',
})
export class ResizedDirective extends ResourceOwner implements OnDestroy, OnChanges, ResizeListener {
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

    constructor(resizeService: ResizeService, element: ElementRef,
        private readonly zone: NgZone,
    ) {
        super();

        this.own(resizeService.listen(element.nativeElement, this));
    }

    public ngOnChanges() {
        const minWidth = parseInt(this.minWidth as any, 10);
        const maxWidth = parseInt(this.maxWidth as any, 10);

        if (minWidth > 0 && maxWidth > 0) {
            this.condition = rect => rect.width < minWidth! || rect.width > maxWidth!;
        } else if (maxWidth > 0) {
            this.condition = rect => rect.width > maxWidth!;
        } else if (minWidth > 0) {
            this.condition = rect => rect.width < minWidth!;
        } else {
            this.condition = undefined;
        }
    }

    public onResize(rect: ClientRect) {
        if (this.condition) {
            const value = this.condition(rect);

            if (this.conditionValue !== value) {
                this.zone.run(() => {
                    this.resizeCondition.emit(value);
                });

                this.conditionValue = value;
            }
        } else {
            this.zone.run(() => {
                this.resize.emit(rect);
            });
        }
    }
}
