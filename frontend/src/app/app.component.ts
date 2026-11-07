/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, inject, Injector } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AnalyticsService, CopyGlobalDirective, DialogRendererComponent, RootViewComponent, syncChartTheme, ThemeService, TourGuideComponent, TourTemplateComponent, TranslatePipe } from '@app/shared';

@Component({
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
    private readonly injector = inject(Injector);

    public isLoaded?: boolean | null;

    constructor() {
        this.injector.get(AnalyticsService);
        syncChartTheme(this.injector.get(ThemeService));
    }
}
