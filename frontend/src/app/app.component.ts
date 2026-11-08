/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, Injector, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AnalyticsService, CopyGlobalDirective, DialogRendererComponent, RootViewComponent, syncChartTheme, ThemeService, TourGuideComponent, TourTemplateComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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

    protected readonly isLoaded = signal(false);

    constructor() {
        this.injector.get(AnalyticsService);
        syncChartTheme(this.injector.get(ThemeService));
    }
}
