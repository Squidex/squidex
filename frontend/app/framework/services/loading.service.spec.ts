/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Event, NavigationError, NavigationStart } from '@angular/router';
import { Subject } from 'rxjs';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
    const events = new Subject<Event>();

    it('should instantiate', () => {
        const loadingService = new LoadingService(<any>{ events });

        expect(loadingService).toBeDefined();

        loadingService.ngOnDestroy();
    });

    it('should set to loaded', () => {
        const loadingService = new LoadingService(<any>{ events });

        let state = false;

        loadingService.loading.subscribe(v => {
            state = v;
        });
        loadingService.startLoading('1');

        expect(state).toBeTruthy();
    });

    it('should set to loaded on navigation start', () => {
        const loadingService = new LoadingService(<any>{ events });

        let state = false;

        loadingService.loading.subscribe(v => {
            state = v;
        });

        events.next(new NavigationStart(0, ''));

        expect(state).toBeTruthy();
    });

    it('should not unset from loaded immediately', () => {
        const loadingService = new LoadingService(<any>{ events });

        let state = false;

        loadingService.loading.subscribe(v => {
            state = v;
        });
        loadingService.startLoading('1');
        loadingService.completeLoading('1');

        expect(state).toBeTruthy();
    });

    it('should not unset from loaded delayed', (cb) => {
        const loadingService = new LoadingService(<any>{ events });

        let state = false;

        loadingService.loading.subscribe(v => {
            state = v;
        });
        loadingService.startLoading('1');
        loadingService.completeLoading('1');

        setTimeout(() => {
            expect(state).toBeFalsy();

            cb();
        }, 400);
    });

    it('should not unset from loaded delayed on navigation event', (cb) => {
        const loadingService = new LoadingService(<any>{ events });

        let state = false;

        loadingService.loading.subscribe(v => {
            state = v;
        });
        events.next(new NavigationStart(0, ''));
        events.next(new NavigationError(0, '', 0));

        setTimeout(() => {
            expect(state).toBeFalsy();

            cb();
        }, 400);
    });

    it('should set back to loaded after several completions', (cb) => {
        const loadingService = new LoadingService(<any>{ events });

        let state = false;

        loadingService.loading.subscribe(v => {
            state = v;
        });
        loadingService.startLoading('1');
        loadingService.completeLoading('1');
        loadingService.completeLoading('1');
        loadingService.startLoading('2');

        setTimeout(() => {
            expect(state).toBeTruthy();

            cb();
        }, 400);
    });
});
