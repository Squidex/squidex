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
import { AppComponent } from './app.component';
import { routing } from './app.routes';
import { ApiUrlConfig, CurrencyConfig, DateHelper, DecimalSeparatorConfig, LocalizerService, SqxFrameworkModule, SqxSharedModule, TitlesConfig, UIOptions } from './shared';
import { SqxShellModule } from './shell';

DateHelper.setlocale(window['options']?.more?.culture);

export function configApiUrl() {
    const baseElements = document.getElementsByTagName('base');

    let baseHref = null;

    if (baseElements.length > 0) {
        baseHref = baseElements[0].href;
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
    return new TitlesConfig(undefined, 'i18n:common.product');
}

export function configDecimalSeparator() {
    return new DecimalSeparatorConfig('.');
}

export function configCurrency() {
    return new CurrencyConfig('EUR', '€', true);
}

export function configTranslations() {
    if (process.env.NODE_ENV === 'production') {
        return new LocalizerService(window['texts']);
    } else {
        const culture = DateHelper.getLocale();

        return new LocalizerService(require(`./../../backend/i18n/frontend_${culture}.json`)).logMissingKeys();
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
        routing
    ],
    declarations: [
        AppComponent
    ],
    providers: [
        { provide: ApiUrlConfig, useFactory: configApiUrl },
        { provide: CurrencyConfig, useFactory: configCurrency },
        { provide: DecimalSeparatorConfig, useFactory: configDecimalSeparator },
        { provide: TitlesConfig, useFactory: configTitles },
        { provide: LocalizerService, useFactory: configTranslations },
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