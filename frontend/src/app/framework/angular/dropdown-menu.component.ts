/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkTrapFocus  } from '@angular/cdk/a11y';
import { ChangeDetectionStrategy, Component, ElementRef, input } from '@angular/core';

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
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DropdownMenuComponent {
    /** Additional CSS classes for the host element. */
    public readonly class = input<string>();

    public get nativeElement() {
        return this.element.nativeElement;
    }

    constructor(
        private readonly element: ElementRef,
    ) {
    }
}
