/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { APP_BASE_HREF } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { enableProdMode, ErrorHandler } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { ActivatedRouteSnapshot, BaseRouteReuseStrategy, provideRouter, RouteReuseStrategy } from '@angular/router';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { TourService as BaseTourService } from 'ngx-ui-tour-core';
import { APP_ROUTES } from '@app/app.routes';
import { ApiUrlConfig, authInterceptor, buildTasks, cachingInterceptor, DateHelper, GlobalErrorHandler, loadingInterceptor, LocalizerService, TASK_CONFIGURATION, TitlesConfig, TourService, UIOptions } from '@app/shared';
import { AppComponent } from './app/app.component';
import { environment } from './environments/environment';

const options = (window as any)['options'] || {};

DateHelper.setlocale(options.more?.culture);

function basePath() {
    const baseElements = document.getElementsByTagName('base');

    let baseHref: string = null!;

    if (baseElements.length > 0) {
        baseHref = baseElements[0].href;
    }

    if (baseHref.indexOf('http') === 0) {
        baseHref = new URL(baseHref).pathname;
    }

    if (!baseHref) {
        baseHref = '';
    }

    let path = options.embedPath || '/';

    while (baseHref.endsWith('/')) {
        baseHref = baseHref.substring(0, baseHref.length - 1);
    }

    return `${baseHref}${path}`;
}

function configApiUrl() {
    const baseElements = document.getElementsByTagName('base');

    let baseHref: string = null!;

    if (baseElements.length > 0) {
        baseHref = baseElements[0].href;
    }

    if (!baseHref) {
        baseHref = '/';
    }

    if (baseHref.indexOf('http') === 0) {
        return new ApiUrlConfig(baseHref);
    } else {
        return new ApiUrlConfig(`${window.location.protocol}//${window.location.host}${baseHref}`);
    }
}

function configUIOptions() {
    return new UIOptions(options);
}

function configTitles() {
    return new TitlesConfig(undefined, 'i18n:common.product');
}

function configLocalizerService() {
    return new LocalizerService(environment.textResolver());
}

export class AppRouteReuseStrategy extends BaseRouteReuseStrategy {
    public shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot) {
       return (future.routeConfig === curr.routeConfig) || (future.data['reuseId'] && future.data['reuseId'] === curr.data['reuseId']);
    }
}

if (environment.production) {
    enableProdMode();
}

bootstrapApplication(AppComponent, {
    providers: [
        provideAnimations(),
        provideCharts(withDefaultRegisterables()),
        provideHttpClient(
            withInterceptors([
                loadingInterceptor,
                cachingInterceptor,
                authInterceptor,
            ]),
        ),
        provideRouter(APP_ROUTES),
        {
            provide: RouteReuseStrategy,
            useClass: AppRouteReuseStrategy,
        },
        {
            provide: ApiUrlConfig,
            useFactory: configApiUrl,
        },
        {
            provide: LocalizerService,
            useFactory: configLocalizerService,
        },
        {
            provide: TitlesConfig,
            useFactory: configTitles,
        },
        {
            provide: UIOptions,
            useFactory: configUIOptions,
        },
        {
            provide: APP_BASE_HREF,
            useValue: basePath(),
        },
        {
            provide: BaseTourService,
            useClass: TourService,
        },
        {
            provide: ErrorHandler,
            useClass: GlobalErrorHandler,
            multi: false,
        },
        {
            provide: TASK_CONFIGURATION,
            useFactory: buildTasks,
            multi: false,
        },
    ],
});
