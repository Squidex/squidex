/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, OnInit } from '@angular/core';

@Directive({
    selector: '.sqx-cloak'
})
export class CloakDirective implements OnInit {
    constructor(
        private readonly element: ElementRef
    ) {
    }

    public ngOnInit() {
        this.element.nativeElement.classList.remove('sqx-cloak');
    }
}