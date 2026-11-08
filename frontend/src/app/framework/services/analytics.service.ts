/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Types } from '../internal';

type TrackEvent = (name: string, properties: any) => void;
type TrackPage = (url: string) => void;

@Service()
export class AnalyticsService {
    private readonly router = inject(Router);
    private readonly globalTrackEvent?: TrackEvent;
    private readonly globalTrackPage?: TrackPage;

    constructor() {
        this.globalTrackEvent = (window as any)['trackEvent'];
        this.globalTrackPage = (window as any)['trackEvent'];

        this.router.events.subscribe(event => {
            if (Types.is(event, NavigationEnd)) {
                this.globalTrackPage?.(event.urlAfterRedirects);
            }
        });
    }

    public trackEvent(name: string, properties: any) {
        this.globalTrackEvent?.(name, properties);
    }
}