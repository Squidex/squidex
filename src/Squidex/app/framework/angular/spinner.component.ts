/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef } from '@angular/core';

declare var Spinner: any;

@Component({
    selector: 'sqx-spinner',
    template: ''
})
export class SpinnerComponent {
    constructor(element: ElementRef) {
        const mediumOptions = {
            lines: 12,
            length: 5,
            width: 2,
            radius: 6,
            corners: 1,
            rotate: 0,
            direction: 1,
            color: '#000',
            speed: 1.5,
            trail: 40,
            shadow: false,
            hwaccel: false,
            className: 'spinner',
            zIndex: 0,
            position: 'relative'
        };

        element.nativeElement.classList.add('spinner-medium');

        new Spinner(mediumOptions).spin(element.nativeElement);
    }
}