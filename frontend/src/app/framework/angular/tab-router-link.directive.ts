/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Directive({
    selector: '[sqxTabRouterLink]',
})
export class TabRouterlinkDirective {
    @Input('sqxTabRouterLink')
    public commands!: any[];

    constructor(
        private readonly router: Router,
        private readonly route: ActivatedRoute,
    ) {
    }

    @HostListener('click', ['$event'])
    protected onClick(event: MouseEvent) {
        const escaped = this.commands.map(x => encodeURIComponent(x));

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
