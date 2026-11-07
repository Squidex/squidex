/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Event, NavigationError, NavigationStart, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
    const events = new Subject<Event>();

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                { provide: Router, useValue: { events } },
            ],
        });
    });

    it('should instantiate', () => {
        const loadingService = TestBed.inject(LoadingService);

        expect(loadingService).toBeDefined();
    });

    it('should set to loaded', () => {
        const loadingService = TestBed.inject(LoadingService);
        loadingService.scheduler = action => action();

        loadingService.startLoading('1');

        expect(loadingService.loading()).toBeTruthy();
    });

    it('should set to loaded on navigation start', () => {
        const loadingService = TestBed.inject(LoadingService);
        loadingService.scheduler = action => action();

        events.next(new NavigationStart(0, ''));

        expect(loadingService.loading()).toBeTruthy();
    });

    it('should not unset from loaded immediately', () => {
        const loadingService = TestBed.inject(LoadingService);

        loadingService.startLoading('1');
        loadingService.completeLoading('1');

        expect(loadingService.loading()).toBeTruthy();
    });

    it('should not unset from loaded delayed', async () => {
        const loadingService = TestBed.inject(LoadingService);
        loadingService.scheduler = action => action();

        loadingService.startLoading('1');
        loadingService.completeLoading('1');

        expect(loadingService.loading()).toBeFalsy();
    });

    it('should not unset from loaded delayed on navigation event', async () => {
        const loadingService = TestBed.inject(LoadingService);
        loadingService.scheduler = action => action();

        events.next(new NavigationStart(0, ''));
        events.next(new NavigationError(0, '', 0));

        expect(loadingService.loading()).toBeFalsy();
    });

    it('should set back to loaded after several completions', async () => {
        const loadingService = TestBed.inject(LoadingService);
        loadingService.scheduler = action => action();

        loadingService.startLoading('1');
        loadingService.completeLoading('1');
        loadingService.completeLoading('1');
        loadingService.startLoading('2');

        expect(loadingService.loading()).toBeTruthy();
    });
});
