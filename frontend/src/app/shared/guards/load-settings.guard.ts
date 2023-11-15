/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UIState } from '../state/ui.state';

@Injectable()
export class LoadSettingsGuard  {
    constructor(
        private readonly uiState: UIState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.uiState.load().pipe(map(() => true));
    }
}
