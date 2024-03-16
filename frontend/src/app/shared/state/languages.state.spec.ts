/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { AppLanguagesPayload, AppLanguagesService, DialogService, LanguageDto, LanguagesService, LanguagesState, versioned } from '@app/shared/internal';
import { createLanguages } from './../services/app-languages.service.spec';
import { TestValues } from './_test-helpers';

describe('LanguagesState', () => {
    const {
        app,
        appsState,
        newVersion,
        version,
    } = TestValues;

    const languageDE = new LanguageDto('de', 'German');
    const languageEN = new LanguageDto('en', 'English');
    const languageIT = new LanguageDto('it', 'Italian');
    const languageES = new LanguageDto('es', 'Spanish');

    const oldLanguages = createLanguages('en', 'de');

    let dialogs: IMock<DialogService>;
    let languagesService: IMock<AppLanguagesService>;
    let languagesState: LanguagesState;
    let allLanguagesService: IMock<LanguagesService>;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        allLanguagesService = Mock.ofType<LanguagesService>();
        allLanguagesService.setup(x => x.getLanguages())
            .returns(() => of([languageDE, languageEN, languageIT, languageES])).verifiable();

        languagesService = Mock.ofType<AppLanguagesService>();
        languagesService.setup(x => x.getLanguages(app))
            .returns(() => of({ payload: oldLanguages, version })).verifiable();

        languagesState = new LanguagesState(languagesService.object, appsState.object, dialogs.object, allLanguagesService.object);
    });

    afterEach(() => {
        languagesService.verifyAll();

        allLanguagesService.verifyAll();
    });

    describe('Loading', () => {
        it('should load languages', () => {
            languagesState.load().subscribe();

            expect(languagesState.snapshot.languages).toEqual([
                {
                    language: oldLanguages.items[0],
                    fallbackLanguages: [oldLanguages.items[1]],
                    fallbackLanguagesNew: [],
                }, {
                    language: oldLanguages.items[1],
                    fallbackLanguages: [oldLanguages.items[0]],
                    fallbackLanguagesNew: [],
                },
            ]);
            expect(languagesState.snapshot.allLanguagesNew).toEqual([languageIT, languageES]);
            expect(languagesState.snapshot.isLoaded).toBeTruthy();
            expect(languagesState.snapshot.isLoading).toBeFalsy();
            expect(languagesState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            languagesService.setup(x => x.getLanguages(app))
                .returns(() => throwError(() => 'Service Error'));

            languagesState.load().pipe(onErrorResumeNext()).subscribe();

            expect(languagesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            languagesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            languagesState.load().subscribe();
        });

        it('should update languages if language added', () => {
            const updated = createLanguages('de');

            languagesService.setup(x => x.postLanguage(app, It.isAny(), version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            languagesState.add('it').subscribe();

            expectNewLanguages(updated);
        });

        it('should update languages if language updated', () => {
            const updated = createLanguages('de');

            const request = { isMaster: true, isOptional: false, fallback: [] };

            languagesService.setup(x => x.putLanguage(app, oldLanguages.items[1], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            languagesState.update(oldLanguages.items[1], request).subscribe();

            expectNewLanguages(updated);
        });

        it('should update languages if language deleted', () => {
            const updated = createLanguages('de');

            languagesService.setup(x => x.deleteLanguage(app, oldLanguages.items[1], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            languagesState.remove(oldLanguages.items[1]).subscribe();

            expectNewLanguages(updated);
        });

        function expectNewLanguages(updated: AppLanguagesPayload) {
            expect(languagesState.snapshot.languages).toEqual([
                {
                    language: updated.items[0],
                    fallbackLanguages: [],
                    fallbackLanguagesNew: [],
                },
            ]);
            expect(languagesState.snapshot.allLanguagesNew).toEqual([languageEN, languageIT, languageES]);
            expect(languagesState.snapshot.version).toEqual(newVersion);
        }
    });
});
