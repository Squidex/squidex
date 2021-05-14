/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnDestroy } from '@angular/core';
import { TitleService } from '@app/framework/internal';

@Component({
    selector: 'sqx-title',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TitleComponent implements OnDestroy {
    private previous: any;

    @Input()
    public set message(value: any) {
        this.titleService.push(value, this.previous);

        this.previous = value;
    }

    constructor(
        private readonly titleService: TitleService,
    ) {
    }

    public ngOnDestroy() {
        if (this.previous) {
            this.titleService.pop();
        }
    }
}
