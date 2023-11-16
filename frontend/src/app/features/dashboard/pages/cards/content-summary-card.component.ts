/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { AppDto, ContentsService, StatefulComponent, TranslatePipe, Types } from '@app/shared';

interface State {
    // The number of items.
    itemCount: number;
}

@Component({
    standalone: true,
    selector: 'sqx-content-summary-card',
    styleUrls: ['./content-summary-card.component.scss'],
    templateUrl: './content-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class ContentSummaryCardComponent extends StatefulComponent<State> implements OnInit {
    @Input({ required: true })
    public app!: AppDto;

    @Input()
    public options?: any;

    constructor(
        private readonly contentsService: ContentsService,
    ) {
        super({
            itemCount: 0,
        });
    }

    public ngOnInit() {
        if (!Types.isString(this.options?.schema)) {
            return;
        }

        let query = this.options?.query;

        if (!Types.isObject(query)) {
            query = {};
        }

        query.take = 0;

        this.contentsService.getContents(this.app.name, this.options.schema, { query })
            .subscribe({
                next: ({ total: itemCount }) => {
                    this.next({ itemCount });
                },
                error: () => {
                    this.next({ itemCount: 0 });
                },
            });
    }
}
