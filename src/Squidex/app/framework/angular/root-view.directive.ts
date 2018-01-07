/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ViewContainerRef } from '@angular/core';

import { RootViewService } from './../services/root-view.service';

@Directive({
    selector: '[sqxRootView]'
})
export class RootViewDirective {
    constructor(public viewContainer: ViewContainerRef, rootViewService: RootViewService) {
        rootViewService.init(viewContainer);
    }
}