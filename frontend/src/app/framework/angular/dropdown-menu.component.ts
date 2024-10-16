/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-host-metadata-property */

import { ChangeDetectionStrategy, Component, ElementRef, HostBinding, Input } from '@angular/core';
import { fadeAnimation } from './animations';

@Component({
    standalone: true,
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
