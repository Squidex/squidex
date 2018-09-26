/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppLanguageDto,
    AppLanguagesDto,
    AppLanguagesService,
    AppsState,
    DialogService,
    ImmutableArray,
    LanguageDto,
    LanguagesService,
    LanguagesState,
    UpdateAppLanguageDto,
    Version,
    Versioned
} from '@app/shared';

describe('LanguagesState', () => {
    const app = 'my-app';
    const version = new Version('1');
    const newVersion = new Version('2');

    const languageDE = new LanguageDto('de', 'German');
    const languageEN = new LanguageDto('en', 'English');
    const languageIT = new LanguageDto('it', 'Italian');
    const languageES = new LanguageDto('es', 'Spanish');

    const oldLanguages = [
        new AppLanguageDto(languageEN.iso2Code, languageEN.englishName, true,  false, []),
        new AppLanguageDto(languageDE.iso2Code, languageDE.englishName, false, true,  [languageEN.iso2Code])
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let allLanguagesService: IMock<LanguagesService>;
    let languagesService: IMock<AppLanguagesService>;
    let languagesState: LanguagesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        allLanguagesService = Mock.ofType<LanguagesService>();

        allLanguagesService.setup(x => x.getLanguages())
            .returns(() => of([languageDE, languageEN, languageIT, languageES]));

        languagesService = Mock.ofType<AppLanguagesService>();

        languagesService.setup(x => x.getLanguages(app))
            .returns(() => of(new AppLanguagesDto(oldLanguages, version)));

        languagesState = new LanguagesState(languagesService.object, appsState.object, dialogs.object, allLanguagesService.object);
        languagesState.load().subscribe();
    });

    it('should load languages', () => {
        expect(languagesState.snapshot.languages.values).toEqual([
           {
               language: oldLanguages[0],
               fallbackLanguages: ImmutableArray.empty(),
               fallbackLanguagesNew: ImmutableArray.of([oldLanguages[1]])
           }, {
               language: oldLanguages[1],
               fallbackLanguages: ImmutableArray.of([oldLanguages[0]]),
               fallbackLanguagesNew: ImmutableArray.empty()
           }
        ]);
        expect(languagesState.snapshot.allLanguagesNew.values).toEqual([languageIT, languageES]);
        expect(languagesState.snapshot.isLoaded).toBeTruthy();
        expect(languagesState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        languagesState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add language to snapshot when assigned', () => {
        const newLanguage = new AppLanguageDto(languageIT.iso2Code, languageIT.englishName, false, false, []);

        languagesService.setup(x => x.postLanguage(app, It.isAny(), version))
            .returns(() => of(new Versioned<AppLanguageDto>(newVersion, newLanguage)));

        languagesState.add(languageIT).subscribe();

        expect(languagesState.snapshot.languages.values).toEqual([
            {
                language: oldLanguages[0],
                fallbackLanguages: ImmutableArray.empty(),
                fallbackLanguagesNew: ImmutableArray.of([oldLanguages[1], newLanguage])
            }, {
                language: oldLanguages[1],
                fallbackLanguages: ImmutableArray.of([oldLanguages[0]]),
                fallbackLanguagesNew: ImmutableArray.of([newLanguage])
            }, {
                language: newLanguage,
                fallbackLanguages: ImmutableArray.of(),
                fallbackLanguagesNew: ImmutableArray.of([oldLanguages[0], oldLanguages[1]])
            }
         ]);
         expect(languagesState.snapshot.allLanguagesNew.values).toEqual([languageES]);
         expect(languagesState.snapshot.version).toEqual(newVersion);
    });

    it('should update language in snapshot when updated', () => {
        const request = new UpdateAppLanguageDto(true, false, []);

        languagesService.setup(x => x.putLanguage(app, oldLanguages[1].iso2Code, request, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        languagesState.update(oldLanguages[1], request).subscribe();

        const newLanguage1 = new AppLanguageDto(languageDE.iso2Code, languageDE.englishName, true,  false, []);
        const newLanguage2 = new AppLanguageDto(languageEN.iso2Code, languageEN.englishName, false, false, []);

        expect(languagesState.snapshot.languages.values).toEqual([
           {
               language: newLanguage1,
               fallbackLanguages: ImmutableArray.empty(),
               fallbackLanguagesNew: ImmutableArray.of([newLanguage2])
           }, {
               language: newLanguage2,
               fallbackLanguages: ImmutableArray.empty(),
               fallbackLanguagesNew: ImmutableArray.of([newLanguage1])
           }
        ]);
        expect(languagesState.snapshot.allLanguagesNew.values).toEqual([languageIT, languageES]);
        expect(languagesState.snapshot.version).toEqual(newVersion);
    });

    it('should remove language from snapshot when deleted', () => {
        languagesService.setup(x => x.deleteLanguage(app, oldLanguages[1].iso2Code, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        languagesState.remove(oldLanguages[1]).subscribe();

        expect(languagesState.snapshot.languages.values).toEqual([
            {
                language: oldLanguages[0],
                fallbackLanguages: ImmutableArray.empty(),
                fallbackLanguagesNew: ImmutableArray.empty()
            }
        ]);
        expect(languagesState.snapshot.allLanguagesNew.values).toEqual([languageDE, languageIT, languageES]);
        expect(languagesState.snapshot.version).toEqual(newVersion);
    });
});