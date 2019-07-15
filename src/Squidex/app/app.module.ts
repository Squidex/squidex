/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ApplicationRef, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import { AppComponent } from './app.component';

import {
    AnalyticsIdConfig,
    ApiUrlConfig,
    CurrencyConfig,
    DecimalSeparatorConfig,
    SqxFrameworkModule,
    SqxSharedModule,
    TitlesConfig,
    UIOptions
} from './shared';

import { SqxShellModule } from './shell';

import { routing } from './app.routes';

export function configApiUrl() {
    let bases = document.getElementsByTagName('base');
    let baseHref = null;

    if (bases.length > 0) {
        baseHref = bases[0].href;
    }

    if (!baseHref) {
        baseHref = '/';
    }

    if (baseHref.indexOf(window.location.protocol) === 0) {
        return new ApiUrlConfig(baseHref);
    } else {
        return new ApiUrlConfig(window.location.protocol + '//' + window.location.host + baseHref);
    }
}

export function configUIOptions() {
    return new UIOptions(window['options']);
}

export function configTitles() {
    return new TitlesConfig({}, undefined, 'Squidex Headless CMS');
}

export function configAnalyticsId() {
    return new AnalyticsIdConfig('UA-99989790-2');
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
        BrowserAnimationsModule,
        DndModule.forRoot(),
        HttpClientModule,
        FormsModule,
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        SqxFrameworkModule.forRoot(),
        SqxSharedModule.forRoot(),
        SqxShellModule,
        routing
    ],
    declarations: [
        AppComponent
    ],
    providers: [
        { provide: AnalyticsIdConfig, useFactory: configAnalyticsId },
        { provide: ApiUrlConfig, useFactory: configApiUrl },
        { provide: CurrencyConfig, useFactory: configCurrency },
        { provide: DecimalSeparatorConfig, useFactory: configDecimalSeparator },
        { provide: TitlesConfig, useFactory: configTitles },
        { provide: UIOptions, useFactory: configUIOptions }
    ],
    entryComponents: [AppComponent]
})
export class AppModule {
    public ngDoBootstrap(appRef: ApplicationRef) {
        try {
            appRef.bootstrap(AppComponent);
        } catch (e) {
            console.log('Application element not found');
        }
    }
}