/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, inject, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Directive({
    selector: '[sqxTabRouterLink]',
    host: {
        '(click)': 'onClick($event)',
    },
})
export class TabRouterlinkDirective {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);

    /** The router commands to navigate to; opens a new tab when ctrl or meta is pressed. */
    public readonly commands = input.required<any[]>({ alias: 'sqxTabRouterLink' });

    protected onClick(event: MouseEvent) {
        const escaped = this.commands().map(x => encodeURIComponent(x));

        const urlTree = this.router.createUrlTree(escaped, {
            relativeTo: this.route,
        });

        if (event.ctrlKey || event.metaKey) {
            const url = this.router.serializeUrl(urlTree);

            window.open(url, '_blank');
        } else {
            this.router.navigateByUrl(urlTree);
        }
    }
}
