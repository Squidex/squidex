/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html'
})
export class AppComponent {
    public isLoaded = false;
}