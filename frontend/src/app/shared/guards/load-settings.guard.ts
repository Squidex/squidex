/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { UIState } from '../state/ui.state';

export const loadSettingsGuard = () => {
    const uiState = inject(UIState);

    return uiState.load().pipe(map(() => true));
};
