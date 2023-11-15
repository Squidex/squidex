/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LanguagesState } from '@app/shared/internal';

@Injectable()
export class LoadLanguagesGuard  {
    constructor(
        private readonly languagesState: LanguagesState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.languagesState.load().pipe(map(_ => true));
    }
}
