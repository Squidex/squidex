/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TitleService, Types } from '@app/framework/internal';

@Component({
    standalone: true,
    selector: 'sqx-title',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TitleComponent implements OnDestroy {
    private previousIndex: undefined | number;

    @Input()
    public url: any[] = ['./'];

    @Input({ required: true })
    public message!: string;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly titleService: TitleService,
    ) {
    }

    public ngOnChanges() {
        const routeTree = this.router.createUrlTree(this.url, { relativeTo: this.route });
        const routeUrl = this.router.serializeUrl(routeTree);

        this.previousIndex = this.titleService.push(this.message, this.previousIndex, routeUrl);
    }

    public ngOnDestroy() {
        if (Types.isNumber(this.previousIndex)) {
            this.titleService.pop();
        }
    }
}
