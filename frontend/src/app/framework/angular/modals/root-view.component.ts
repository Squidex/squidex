/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, viewChild, ViewContainerRef } from '@angular/core';

@Component({
    selector: 'sqx-root-view',
    styleUrls: ['./root-view.component.scss'],
    templateUrl: './root-view.component.html',
})
export class RootViewComponent {
    public readonly viewContainer = viewChild.required('element', { read: ViewContainerRef });
}
