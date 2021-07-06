/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LanguagesState } from './../state/languages.state';

@Injectable()
export class LoadLanguagesGuard implements CanActivate {
    constructor(
        private readonly languagesState: LanguagesState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.languagesState.load().pipe(map(_ => true));
    }
}
