/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, DestroyRef, effect, inject, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TitleService, Types } from '@app/framework/internal';

@Component({
    selector: 'sqx-title',
    template: '',
})
export class TitleComponent {
    private readonly destroyRef = inject(DestroyRef);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly titleService = inject(TitleService);
    private previousIndex: undefined | number;

    /** The route of the title in the breadcrumb, relative to the current route. */
    public readonly url = input<any[]>(['./']);

    /** The title to push to the title service. */
    public readonly message = input.required<string>();

    constructor() {
        effect(() => {
            const message = this.message();
            const routeTree = this.router.createUrlTree(this.url(), { relativeTo: this.route });
            const routeUrl = this.router.serializeUrl(routeTree);

            this.previousIndex = this.titleService.push(message, this.previousIndex, routeUrl);
        });

        this.destroyRef.onDestroy(() => {
            if (Types.isNumber(this.previousIndex)) {
                this.titleService.pop();
            }
        });
    }
}
