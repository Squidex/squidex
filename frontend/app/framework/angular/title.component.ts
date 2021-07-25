/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { ChangeDetectionStrategy, Component, Input, OnChanges, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TitleService } from '@app/framework/internal';

@Component({
    selector: 'sqx-title[message]',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TitleComponent implements OnDestroy, OnChanges {
    private previous: any;

    @Input()
    public url: any[] = ['./'];

    @Input()
    public message: string;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly titleService: TitleService,
    ) {
    }

    public ngOnChanges() {
        const routeTree = this.router.createUrlTree(this.url, { relativeTo: this.route });
        const routeUrl = this.router.serializeUrl(routeTree);

        this.titleService.push(this.message, this.previous, routeUrl);

        this.previous = this.message;
    }

    public ngOnDestroy() {
        if (this.previous) {
            this.titleService.pop();
        }
    }
}
