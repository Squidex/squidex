/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostBinding, Input, NgZone, OnInit, Renderer2 } from '@angular/core';
import { Subscriptions } from '@app/framework/internal';

@Directive({
    selector: '[sqxImageUrl]',
    standalone: true,
})
export class ImageUrlDirective implements  OnInit {
    private readonly subscriptions = new Subscriptions();

    @Input('sqxImageUrl') @HostBinding('attr.src')
    public imageUrl!: string;

    constructor(
        private readonly zone: NgZone,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnInit() {
        this.zone.runOutsideAngular(() => {
            this.subscriptions.add(
                this.renderer.listen(this.element.nativeElement, 'load', () => {
                    this.onLoad();
                }));

            this.subscriptions.add(
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
