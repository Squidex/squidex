/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LanguagesState } from '@app/shared';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { LoadLanguagesGuard } from './load-languages.guard';

describe('LoadLanguagesGuard', () => {
    let languagesState: IMock<LanguagesState>;
    let languageGuard: LoadLanguagesGuard;

    beforeEach(() => {
        languagesState = Mock.ofType<LanguagesState>();
        languageGuard = new LoadLanguagesGuard(languagesState.object);
    });

    it('should load languages', () => {
        languagesState.setup(x => x.load())
            .returns(() => of(null));

        let result = false;

        languageGuard.canActivate().subscribe(value => {
            result = value;
        });

        expect(result).toBeTruthy();

        languagesState.verify(x => x.load(), Times.once());
    });
});
