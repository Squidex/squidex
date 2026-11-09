/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkTrapFocus  } from '@angular/cdk/a11y';
import { Component, ElementRef, inject, input } from '@angular/core';

@Component({
    selector: 'sqx-dropdown-menu',
    styleUrls: ['./dropdown-menu.component.scss'],
    templateUrl: './dropdown-menu.component.html',
    host: {
        class: 'dropdown-menu',
        ['[class]']: 'class()',
        ['animate.enter']: 'fade-in',
        ['animate.leave']: 'fade-out',
    },
    hostDirectives: [
        CdkTrapFocus ,
    ],
})
export class DropdownMenuComponent {
    private readonly element = inject(ElementRef);

    /** Additional CSS classes for the host element. */
    public readonly class = input<string>();

    public get nativeElement() {
        return this.element.nativeElement;
    }
}
