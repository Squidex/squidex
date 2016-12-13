/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

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
    PanelService,
    SqxFrameworkModule,
    SqxSharedModule,
    TitlesConfig,
    TitleService,
    UsersProviderService,
    UsersService
} from './shared';

import { SqxShellModule } from './shell';

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

@NgModule({
    imports: [
        BrowserModule,
        SqxFrameworkModule,
        SqxSharedModule,
        SqxShellModule,
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
        PanelService,
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