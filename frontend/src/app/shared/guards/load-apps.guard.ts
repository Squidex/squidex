/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { AppsState } from '../state/apps.state';

export const loadAppsGuard = () => {
    const appsState = inject(AppsState);

    return appsState.load().pipe(map(() => true));
};
