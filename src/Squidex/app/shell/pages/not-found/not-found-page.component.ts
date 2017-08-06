/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
    selector: 'sqx-not-found-page',
    styleUrls: ['./not-found-page.component.scss'],
    templateUrl: './not-found-page.component.html'
})
export class NotFoundPageComponent {
    constructor(
        private readonly location: Location
    ) {
    }

    public back() {
        this.location.back();
    }
}