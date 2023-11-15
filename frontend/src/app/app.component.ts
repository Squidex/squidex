/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CopyGlobalDirective, DialogRendererComponent, RootViewComponent, TourGuideComponent, TourTemplateComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    standalone: true,
    imports: [
        CopyGlobalDirective,
        RootViewComponent,
        RouterOutlet,
        NgIf,
        TourGuideComponent,
        TourTemplateComponent,
        DialogRendererComponent,
        TranslatePipe,
    ],
})
export class AppComponent {
    public isLoaded?: boolean | null;
}
