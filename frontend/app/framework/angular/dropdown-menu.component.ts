/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, ElementRef, HostBinding } from '@angular/core';
import { fadeAnimation } from './animations';

@Component({
    selector: 'sqx-dropdown-menu',
    styleUrls: ['./dropdown-menu.component.scss'],
    templateUrl: './dropdown-menu.component.html',
    host: {
        class: 'dropdown-menu',
    },
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DropdownMenuComponent {
    @HostBinding('@fade')
    public get fade() {
        return true;
    }

    public get nativeElement() {
        return this.element.nativeElement;
    }

    constructor(
        private readonly element: ElementRef,
    ) {
    }
}
