/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/directive-selector */

import { Directive, TemplateRef } from '@angular/core';

@Directive({
    selector: '[sidebarMenu]',
    standalone: true,
})
export class SidebarMenuDirective {
    constructor(
        public readonly templateRef: TemplateRef<unknown>,
    ) {
    }
}