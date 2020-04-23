/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { ApplicationRef, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { AppComponent } from './app.component';
import { routing } from './app.routes';
import { ApiUrlConfig, CurrencyConfig, DecimalSeparatorConfig, SqxFrameworkModule, SqxSharedModule, TitlesConfig, UIOptions } from './shared';
import { SqxShellModule } from './shell';

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
    return new TitlesConfig(undefined, 'Squidex Headless CMS');
}

export function configDecimalSeparator() {
    return new DecimalSeparatorConfig('.');
}

export function configCurrency() {
    return new CurrencyConfig('EUR', '€', true);
}

export function HttpLoaderFactory(http: HttpClient) {
    let languageFolderOption = window['options']['ngxTranslate']['languageFolder'];
    let languageFolder =  !(languageFolderOption) ? './locale/' : languageFolderOption;
    return new TranslateHttpLoader(http, languageFolder, '.json');
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
        TranslateModule.forRoot({
            defaultLanguage: 'en',
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        })
    ],
    declarations: [
        AppComponent
    ],
    providers: [
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