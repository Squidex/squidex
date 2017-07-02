/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, OnInit, Renderer } from '@angular/core';

@Directive({
    selector: '.sqx-cloak'
})
export class CloakDirective implements OnInit {
    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnInit() {
        this.renderer.setElementClass(this.element.nativeElement, 'sqx-cloak', false);
    }
}