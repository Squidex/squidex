/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, input, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TitleService, Types } from '@app/framework/internal';

@Component({
    selector: 'sqx-title',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TitleComponent implements OnDestroy {
    private previousIndex: undefined | number;

    /** The route of the title in the breadcrumb, relative to the current route. */
    public readonly url = input<any[]>(['./']);

    /** The title to push to the title service. */
    public readonly message = input.required<string>();

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly titleService: TitleService,
    ) {
        effect(() => {
            const message = this.message();
            const routeTree = this.router.createUrlTree(this.url(), { relativeTo: this.route });
            const routeUrl = this.router.serializeUrl(routeTree);

            this.previousIndex = this.titleService.push(message, this.previousIndex, routeUrl);
        });
    }

    public ngOnDestroy() {
        if (Types.isNumber(this.previousIndex)) {
            this.titleService.pop();
        }
    }
}
