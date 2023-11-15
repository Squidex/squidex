/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { LanguagesState } from '@app/shared/internal';

export const loadLanguagesGuard = () => {
    const languagesState = inject(LanguagesState);

    return languagesState.load().pipe(map(_ => true));
};
