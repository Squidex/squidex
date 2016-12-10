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
    AppClientsService,
    AppContributorsService,
    AppLanguagesService,
    AppMustExistGuard,
    AppsService,
    AppsStoreService,
    AuthService,
    CurrencyConfig,
    DecimalSeparatorConfig,
    DragService,
    LanguageService,
    LocalStoreService,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    NotificationService,
    SqxFrameworkModule,
    TitlesConfig,
    TitleService,
    UsersProviderService,
    UsersService
} from './shared';

import {
    SqxAppModule,
    SqxAuthModule,
    SqxLayoutModule,
    SqxPublicModule
} from './components';

import { routing } from './app.routes';

export function configApiUrl() {
    return new ApiUrlConfig(window.location.protocol + '//' + window.location.host + '/');
}

export function configTitles() {
    return new TitlesConfig({}, undefined, 'Squidex Headless CMS');
}

export function configDecimalSeparator() {
    return  new DecimalSeparatorConfig('.');
}

export function configCurrency() {
    return new CurrencyConfig('EUR', '€', true);
}

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
        AppClientsService,
        AppContributorsService,
        AppLanguagesService,
        AppsStoreService,
        AppsService,
        AppMustExistGuard,
        AuthService,
        DragService,
        LanguageService,
        LocalStoreService,
        MustBeAuthenticatedGuard,
        MustBeNotAuthenticatedGuard,
        NotificationService,
        TitleService,
        UsersProviderService,
        UsersService,
        { provide: ApiUrlConfig, useFactory: configApiUrl },
        { provide: CurrencyConfig, useFactory: configCurrency },
        { provide: DecimalSeparatorConfig, useFactory: configDecimalSeparator },
        { provide: TitlesConfig, useFactory: configTitles }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }