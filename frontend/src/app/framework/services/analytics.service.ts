/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Types } from '../internal';

type TrackEvent = (name: string, properties: any) => void;
type TrackPage = (url: string) => void;

@Injectable({
    providedIn: 'root',
})
export class AnalyticsService {
    private readonly globalTrackEvent?: TrackEvent;
    private readonly globalTrackPage?: TrackPage;

    constructor(router: Router) {
        this.globalTrackEvent = (window as any)['trackEvent'];
        this.globalTrackPage = (window as any)['trackEvent'];

        router.events.subscribe(event => {
            if (Types.is(event, NavigationEnd)) {
                this.globalTrackPage?.(event.urlAfterRedirects);
            }
        });
    }

    public trackEvent(name: string, properties: any) {
        this.globalTrackEvent?.(name, properties);
    }
}