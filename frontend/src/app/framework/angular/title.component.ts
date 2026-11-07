/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, DestroyRef, effect, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TitleService, Types } from '@app/framework/internal';

@Component({
    selector: 'sqx-title',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TitleComponent {
    private previousIndex: undefined | number;

    /** The route of the title in the breadcrumb, relative to the current route. */
    public readonly url = input<any[]>(['./']);

    /** The title to push to the title service. */
    public readonly message = input.required<string>();

    constructor(
        route: ActivatedRoute,
        router: Router,
        titleService: TitleService,
        destroyRef: DestroyRef,
    ) {
        effect(() => {
            const message = this.message();
            const routeTree = router.createUrlTree(this.url(), { relativeTo: route });
            const routeUrl = router.serializeUrl(routeTree);

            this.previousIndex = titleService.push(message, this.previousIndex, routeUrl);
        });

        destroyRef.onDestroy(() => {
            if (Types.isNumber(this.previousIndex)) {
                titleService.pop();
            }
        });
    }
}
