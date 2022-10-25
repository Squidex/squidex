/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable global-require */
/* eslint-disable import/no-dynamic-require */

import { APP_BASE_HREF, CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ApplicationRef, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRouteSnapshot, BaseRouteReuseStrategy, RouteReuseStrategy, RouterModule } from '@angular/router';
import { environment } from './../environments/environment';
import { AppComponent } from './app.component';
import { routing } from './app.routes';
import { ApiUrlConfig, DateHelper, LocalizerService, SqxFrameworkModule, SqxSharedModule, TitlesConfig, UIOptions } from './shared';
import { SqxShellModule } from './shell';

const options = window['options'] || {};

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
    return new LocalizerService(environment.textResolver()).logMissingKeys(environment.textLogger);
}

export class AppRouteReuseStrategy extends BaseRouteReuseStrategy {
    public shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot) {
       return (future.routeConfig === curr.routeConfig) || (future.data['reuseId'] && future.data['reuseId'] === curr.data['reuseId']);
    }
}

@NgModule({
    imports: [
        BrowserAnimationsModule,
        BrowserModule,
        CommonModule,
        FormsModule,
        HttpClientModule,
        ReactiveFormsModule,
        RouterModule,
        SqxFrameworkModule.forRoot(),
        SqxSharedModule.forRoot(),
        SqxShellModule,
        routing,
    ],
    declarations: [
        AppComponent,
    ],
    providers: [
        { provide: ApiUrlConfig, useFactory: configApiUrl },
        { provide: LocalizerService, useFactory: configLocalizerService },
        { provide: RouteReuseStrategy, useClass: AppRouteReuseStrategy },
        { provide: TitlesConfig, useFactory: configTitles },
        { provide: UIOptions, useFactory: configUIOptions },
        { provide: APP_BASE_HREF, useValue: basePath() },
    ],
})
export class AppModule {
    public ngDoBootstrap(appRef: ApplicationRef) {
        try {
            appRef.bootstrap(AppComponent);
        } catch (e) {
            // eslint-disable-next-line no-console
            console.log('Application element not found.');
        }
    }
}
