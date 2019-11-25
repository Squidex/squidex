/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';
import { timer } from 'rxjs';

import { ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxSyncWidth]'
})
export class SyncWidthDirective extends ResourceOwner implements OnInit, AfterViewInit {
    @Input('sqxSyncWidth')
    public target: HTMLElement;

    constructor(
        private readonly element: ElementRef<HTMLElement>,
        private readonly renderer: Renderer2
    ) {
        super();
    }

    public ngOnInit() {
        this.own(timer(100, 100).subscribe(() => this.reposition()));
    }

    public ngAfterViewInit() {
        this.reposition();
    }

    private reposition() {
        if (!this.target) {
            return;
        }

        const size = this.element.nativeElement.clientWidth;

        this.renderer.setStyle(this.target, 'width', `${size}px`);
    }
}