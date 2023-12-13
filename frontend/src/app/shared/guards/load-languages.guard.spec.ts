/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { LanguagesState } from '@app/shared/internal';
import { loadLanguagesGuard } from './load-languages.guard';

describe('LoadLanguagesGuard', () => {
    let languagesState: IMock<LanguagesState>;

    beforeEach(() => {
        languagesState = Mock.ofType<LanguagesState>();

        TestBed.configureTestingModule({ providers: [{ provide: LanguagesState, useValue: languagesState.object }] });
    });

    bit('should load languages', async () => {
        languagesState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(loadLanguagesGuard());

        expect(result).toBeTruthy();

        languagesState.verify(x => x.load(), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}