/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component } from '@angular/core';

@Component({
    selector: 'sqx-forbidden-page',
    styleUrls: ['./forbidden-page.component.scss'],
    templateUrl: './forbidden-page.component.html',
})
export class ForbiddenPageComponent {
    constructor(
        private readonly location: Location,
    ) {
    }

    public back() {
        this.location.back();
    }
}
