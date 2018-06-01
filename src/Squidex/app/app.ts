/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import './theme/theme.scss';

// icomoon
import './theme/icomoon/style.css';

// Pikaday
import 'pikaday/css/pikaday.css';

// Dragula
import 'dragula/dist/dragula.css';

import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app.module';

if (process.env.ENV === 'production') {
    enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule);