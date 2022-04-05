/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { LanguagesState } from '@app/shared/internal';
import { LoadLanguagesGuard } from './load-languages.guard';

describe('LoadLanguagesGuard', () => {
    let languagesState: IMock<LanguagesState>;
    let languageGuard: LoadLanguagesGuard;

    beforeEach(() => {
        languagesState = Mock.ofType<LanguagesState>();
        languageGuard = new LoadLanguagesGuard(languagesState.object);
    });

    it('should load languages', async () => {
        languagesState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(languageGuard.canActivate());

        expect(result).toBeTruthy();

        languagesState.verify(x => x.load(), Times.once());
    });
});
