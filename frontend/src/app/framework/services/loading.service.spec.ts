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

    it('should not unset from loaded delayed', async () => {
        const loadingService = new LoadingService(<any>{ events });
        loadingService.scheduler = action => action();

        let state = false;
        loadingService.loading.subscribe(v => {
            state = v;
        });
        loadingService.startLoading('1');
        loadingService.completeLoading('1');

        expect(state).toBeFalsy();
    });

    it('should not unset from loaded delayed on navigation event', async () => {
        const loadingService = new LoadingService(<any>{ events });
        loadingService.scheduler = action => action();

        let state = false;
        loadingService.loading.subscribe(v => {
            state = v;
        });
        events.next(new NavigationStart(0, ''));
        events.next(new NavigationError(0, '', 0));

        expect(state).toBeFalsy();
    });

    it('should set back to loaded after several completions', async () => {
        const loadingService = new LoadingService(<any>{ events });
        loadingService.scheduler = action => action();

        let state = false;
        loadingService.loading.subscribe(v => {
            state = v;
        });
        loadingService.startLoading('1');
        loadingService.completeLoading('1');
        loadingService.completeLoading('1');
        loadingService.startLoading('2');

        expect(state).toBeTruthy();
    });
});
