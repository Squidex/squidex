/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import {
    AppLanguagesDto,
    AppLanguagesService,
    Version
} from 'shared';

import { ResolveAppLanguagesGuard } from './resolve-app-languages.guard';
import { RouterMockup } from './router-mockup';

describe('ResolveAppLanguagesGuard', () => {
    const route = {
        params: { },
        parent: {
            params: {
                appName: 'my-app'
            }
        }
    };

    let appLanguagesService: IMock<AppLanguagesService>;

    beforeEach(() => {
        appLanguagesService = Mock.ofType(AppLanguagesService);
    });

    it('should throw if route does not contain parameter', () => {
        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>new RouterMockup());

        expect(() => guard.resolve(<any>{ params: {} }, <any>{})).toThrow('Route must contain app name.');
    });

    it('should navigate to 404 page if languages are not found', (done) => {
        appLanguagesService.setup(x => x.getLanguages('my-app'))
            .returns(() => Observable.of(null!));
        const router = new RouterMockup();

        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if languages loading fails', (done) => {
        appLanguagesService.setup(x => x.getLanguages('my-app'))
            .returns(() => Observable.throw(null!));
        const router = new RouterMockup();

        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return languages if loading succeeded', (done) => {
        const languages = new AppLanguagesDto([], new Version('2'));

        appLanguagesService.setup(x => x.getLanguages('my-app'))
            .returns(() => Observable.of(languages));
        const router = new RouterMockup();

        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(languages.languages);

                done();
            });
    });
});