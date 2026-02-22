/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkTrapFocus  } from '@angular/cdk/a11y';
import { ChangeDetectionStrategy, Component, ElementRef, HostBinding, Input } from '@angular/core';

@Component({
    selector: 'sqx-dropdown-menu',
    styleUrls: ['./dropdown-menu.component.scss'],
    templateUrl: './dropdown-menu.component.html',
    host: {
        class: 'dropdown-menu',
        ['animate.enter']: 'fade-in',
        ['animate.leave']: 'fade-out',
    },
    hostDirectives: [
        CdkTrapFocus ,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DropdownMenuComponent {
    @HostBinding('class')
    @Input()
    public class?: string;

    public get nativeElement() {
        return this.element.nativeElement;
    }

    constructor(
        private readonly element: ElementRef,
    ) {
    }
}
