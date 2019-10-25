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
    template: `
        <sqx-title message="Not Found"></sqx-title>

        <div class="landing-page">
            <img class="splash-image" src="~/../squid.svg?title=FORBIDDEN&text=You%20are%20not%20allowed%20to%20view%20this%20page&background=white&small" />

            <a href="#" (click)="back()">Back to previous page.</a>
        </div>
    `
})
export class ForbiddenPageComponent {
    constructor(
        private readonly location: Location
    ) {
    }

    public back() {
        this.location.back();
    }
}