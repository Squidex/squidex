/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostBinding, Input, NgZone, OnChanges, OnInit, Renderer2 } from '@angular/core';
import { ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxImageUrl]',
})
export class ImageUrlDirective extends ResourceOwner implements OnChanges, OnInit {
    @Input('sqxImageUrl') @HostBinding('attr.src')
    public imageUrl!: string;

    constructor(
        private readonly zone: NgZone,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
        super();
    }

    public ngOnInit() {
        this.zone.runOutsideAngular(() => {
            this.own(
                this.renderer.listen(this.element.nativeElement, 'load', () => {
                    this.onLoad();
                }));

            this.own(
                this.renderer.listen(this.element.nativeElement, 'error', () => {
                    this.onError();
                }));
        });
    }

    public ngOnChanges() {
        this.onError();
    }

    public onLoad() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    public onError() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'hidden');
    }
}
