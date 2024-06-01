/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Injector } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AnalyticsService, CopyGlobalDirective, DialogRendererComponent, RootViewComponent, TourGuideComponent, TourTemplateComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    imports: [
        CopyGlobalDirective,
        DialogRendererComponent,
        RootViewComponent,
        RouterOutlet,
        TourGuideComponent,
        TourTemplateComponent,
        TranslatePipe,
    ],
})
export class AppComponent {
    public isLoaded?: boolean | null;

    constructor(injector: Injector) {
        injector.get(AnalyticsService);
    }
}
