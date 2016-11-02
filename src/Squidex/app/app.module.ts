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
    CurrencyConfig,
    DragService,
    DragServiceFactory,
    DecimalSeparatorConfig,
} from './framework';

import { 
    AppsStoreService,
    AppsService,
    AuthGuard,
    AuthService,
} from './shared';

import {
    SqxAppModule,
    SqxLayoutModule,
    SqxLoginModule
} from './components';

import { SqxFrameworkModule } from './framework';

import { routing } from './app.routes';

const baseUrl = window.location.protocol + '//' + window.location.host + '/';

@Ng2.NgModule({
    imports: [
        Ng2Browser.BrowserModule,
        SqxAppModule,
        SqxLayoutModule,
        SqxLoginModule,
        SqxFrameworkModule,
        routing
    ],
    declarations: [
        AppComponent
    ],
    providers: [
        AppsStoreService,
        AppsService,
        AuthGuard,
        AuthService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig(baseUrl) },
        { provide: CurrencyConfig, useValue: new CurrencyConfig('EUR', '€', true) },
        { provide: DecimalSeparatorConfig, useValue: new DecimalSeparatorConfig('.') },
        { provide: DragService, useFactory: DragServiceFactory }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }