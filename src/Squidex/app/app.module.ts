/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Browser from '@angular/platform-browser';

import { AppComponent } from './app.component';

import {
    ApiUrlConfig,
    AppMustExistGuard,
    AppsStoreService,
    AppsService,
    AuthService,
    CurrencyConfig,
    DragService,
    DragServiceFactory,
    DecimalSeparatorConfig,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    SqxFrameworkModule,
    TitlesConfig,
    TitleService
} from './shared';

import {
    SqxAppModule,
    SqxAuthModule,
    SqxLayoutModule,
    SqxPublicModule
} from './components';

import { routing } from './app.routes';

const baseUrl = window.location.protocol + '//' + window.location.host + '/';;;

@Ng2.NgModule({
    imports: [
        Ng2Browser.BrowserModule,
        SqxAppModule,
        SqxAuthModule,
        SqxLayoutModule,
        SqxFrameworkModule,
        SqxPublicModule,
        routing
    ],
    declarations: [
        AppComponent
    ],
    providers: [
        AppsStoreService,
        AppsService,
        AppMustExistGuard,
        AuthService,
        MustBeAuthenticatedGuard,
        MustBeNotAuthenticatedGuard,
        TitleService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig(baseUrl) },
        { provide: CurrencyConfig, useValue: new CurrencyConfig('EUR', '€', true) },
        { provide: DecimalSeparatorConfig, useValue: new DecimalSeparatorConfig('.') },
        { provide: DragService, useFactory: DragServiceFactory },
        { provide: TitlesConfig, useValue: new TitlesConfig({}, undefined, 'Squidex Headless CMS') }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }