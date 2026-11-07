/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/directive-selector */

import { Directive, inject, TemplateRef } from '@angular/core';

@Directive({
    selector: '[sidebarMenu]',
})
export class SidebarMenuDirective {
    public readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
}